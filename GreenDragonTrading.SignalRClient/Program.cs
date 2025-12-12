using Microsoft.AspNetCore.SignalR.Client;
using Serilog;
using Serilog.Events;

namespace GreenDragonTrading.SignalRClient;

class Program
{
    private static HubConnection? _connection;
    private static int _messageCount = 0;
    private static readonly HashSet<string> _receivedSymbols = new();

    static async Task Main(string[] args)
    {
        // Configure Serilog (giống y chang bên API)
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore.SignalR", LogEventLevel.Debug)
            .MinimumLevel.Override("Microsoft.AspNetCore.Http.Connections", LogEventLevel.Debug)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                "Logs/signalr-client-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        try
        {
            Log.Information("=".PadRight(80, '='));
            Log.Information("Starting GreenDragonTrading SignalR Client");
            Log.Information("=".PadRight(80, '='));

            // Lấy hub URL từ args hoặc dùng default
            var hubUrl = args.Length > 0 ? args[0] : "http://localhost:5146/hubs/marketdata";
            Log.Information("Hub URL: {HubUrl}", hubUrl);

            // Lấy danh sách symbols từ args hoặc dùng default
            var symbolsArg = args.Length > 1 ? args[1] : "FPT";
            var symbols = symbolsArg.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            Log.Information("Subscribing to symbols: {Symbols}", string.Join(", ", symbols));

            // Tạo connection
            _connection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) })
                .ConfigureLogging(logging =>
                {
                    logging.AddSerilog(Log.Logger);
                })
                .Build();

            // Register handlers trước khi connect
            RegisterHandlers();

            // Connect
            Log.Information("Connecting to MarketDataHub...");
            await _connection.StartAsync();
            Log.Information("✅ Connected successfully! ConnectionId: {ConnectionId}", _connection.ConnectionId);

            // Subscribe to symbols
            Log.Information("Subscribing to symbols...");
            await _connection.InvokeAsync("SubscribeToSymbols", symbols);
            Log.Information("✅ Subscribed successfully!");

            // Đợi Ctrl+C để thoát
            Log.Information("");
            Log.Information("📊 Client is running. Press Ctrl+C to stop...");
            Log.Information("");

            // Task để hiển thị stats mỗi 10 giây
            var statsTask = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(10000);
                    Log.Information("📈 Stats: Messages={MessageCount}, Unique Symbols={SymbolCount}", _messageCount, _receivedSymbols.Count);
                }
            });

            // Keep alive
            var tcs = new TaskCompletionSource();
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                tcs.SetResult();
            };

            await tcs.Task;

            // Cleanup
            Log.Information("");
            Log.Information("Disconnecting...");
            await _connection.StopAsync();
            Log.Information("✅ Disconnected successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "❌ Fatal error occurred");
        }
        finally
        {
            Log.Information("=".PadRight(80, '='));
            Log.Information("Final Stats: Total Messages={MessageCount}, Unique Symbols={SymbolCount}", _messageCount, _receivedSymbols.Count);
            Log.Information("Application shutting down");
            Log.Information("=".PadRight(80, '='));
            
            await Log.CloseAndFlushAsync();
        }
    }

    private static void RegisterHandlers()
    {
        if (_connection == null) return;

        // Handler for single market data
        _connection.On<object>("ReceiveMarketData", (data) =>
        {
            _messageCount++;
            
            try
            {
                // Parse symbol từ data
                var dataStr = System.Text.Json.JsonSerializer.Serialize(data);
                var jsonDoc = System.Text.Json.JsonDocument.Parse(dataStr);
                
                string? symbol = null;
                if (jsonDoc.RootElement.TryGetProperty("ticker", out var tickerProp))
                {
                    symbol = tickerProp.GetString();
                }
                else if (jsonDoc.RootElement.TryGetProperty("symbol", out var symbolProp))
                {
                    symbol = symbolProp.GetString();
                }

                if (symbol != null)
                {
                    _receivedSymbols.Add(symbol);
                    Log.Information("📩 [SINGLE] Received market data for {Symbol}", symbol);
                }
                else
                {
                    Log.Information("📩 [SINGLE] Received market data (no symbol)");
                }

                // Log full data
                Log.Debug("Data: {Data}", dataStr);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing single market data");
            }
        });

        // Handler for batch market data
        _connection.On<object[]>("ReceiveBatchMarketData", (dataArray) =>
        {
            try
            {
                Log.Information("📦 [BATCH] Received batch with {Count} items", dataArray.Length);
                
                foreach (var data in dataArray)
                {
                    _messageCount++;
                    
                    var dataStr = System.Text.Json.JsonSerializer.Serialize(data);
                    var jsonDoc = System.Text.Json.JsonDocument.Parse(dataStr);
                    
                    string? symbol = null;
                    if (jsonDoc.RootElement.TryGetProperty("ticker", out var tickerProp))
                    {
                        symbol = tickerProp.GetString();
                    }
                    else if (jsonDoc.RootElement.TryGetProperty("symbol", out var symbolProp))
                    {
                        symbol = symbolProp.GetString();
                    }

                    if (symbol != null)
                    {
                        _receivedSymbols.Add(symbol);
                        Log.Information("   └─ Symbol: {Symbol}", symbol);
                    }

                    Log.Debug("   └─ Data: {Data}", dataStr);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing batch market data");
            }
        });

        // Connection events
        _connection.Closed += async (error) =>
        {
            if (error != null)
            {
                Log.Warning("⚠️ Connection closed with error: {Error}", error.Message);
            }
            else
            {
                Log.Information("Connection closed");
            }
        };

        _connection.Reconnecting += (error) =>
        {
            Log.Warning("🔄 Reconnecting... Error: {Error}", error?.Message ?? "Unknown");
            return Task.CompletedTask;
        };

        _connection.Reconnected += (connectionId) =>
        {
            Log.Information("✅ Reconnected! New ConnectionId: {ConnectionId}", connectionId);
            return Task.CompletedTask;
        };

        Log.Information("✅ Handlers registered");
    }
}
