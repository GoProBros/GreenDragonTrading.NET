using GreenDragonTrading.Application;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.DataImporter;
using GreenDragonTrading.DataImporter.Models;
using GreenDragonTrading.DataImporter.Services;
using GreenDragonTrading.Domain.Interfaces;
using GreenDragonTrading.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

// Build configuration
var basePath = AppContext.BaseDirectory;
var configuration = new ConfigurationBuilder()
    .SetBasePath(basePath)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("=== GreenDragonTrading Data Importer Started ===");

    // Build host
    var host = Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureServices((context, services) =>
        {
            // Register settings
            var importSettings = configuration.GetSection("ImportSettings").Get<ImportSettings>() ?? new ImportSettings();
            services.AddSingleton(importSettings);

            // Register Application layer services
            services.AddApplication();

            // Register Infrastructure layer services
            services.AddInfrastructure(configuration);

            // Register importer services
            services.AddScoped<DailyOhlcvImporter>();
            services.AddScoped<IntradayOhlcvImporter>();
        })
        .Build();

    // Parse command line arguments
    if (args.Length > 0)
    {
        await RunAutomaticModeAsync(host, args);
    }
    else
    {
        await RunInteractiveModeAsync(host);
    }

    Log.Information("=== GreenDragonTrading Data Importer Completed ===");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

static async Task RunInteractiveModeAsync(IHost host)
{
    while (true)
    {
        Console.WriteLine();
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║        GreenDragonTrading OHLCV Data Import Tool              ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine("  [1] Import Daily (D1) data for specific symbol");
        Console.WriteLine("  [2] Import Daily (D1) data for ALL symbols");
        Console.WriteLine("  [3] Import Daily (D1) data for symbol range");
        Console.WriteLine("  [4] Import Intraday (M1) data for specific symbol");
        Console.WriteLine("  [5] Import Intraday (M1) data for ALL symbols");
        Console.WriteLine("  [6] Import Intraday (M1) data for symbol range");
        Console.WriteLine("  [7] View import statistics");
        Console.WriteLine("  [8] Resume incomplete import");
        Console.WriteLine("  [9] Check & Import missing symbols (D1)");
        Console.WriteLine("  [10] Check & Import missing symbols (M1)");
        Console.WriteLine("  [0] Exit");
        Console.WriteLine();
        Console.Write("Select option: ");

        var option = Console.ReadLine()?.Trim();

        try
        {
            switch (option)
            {
                case "1":
                    await ImportSingleSymbolAsync(host);
                    break;
                case "2":
                    await ImportAllSymbolsAsync(host);
                    break;
                case "3":
                    await ImportSymbolRangeAsync(host);
                    break;
                case "4":
                    await ImportSingleIntradaySymbolAsync(host);
                    break;
                case "5":
                    await ImportAllIntradaySymbolsAsync(host);
                    break;
                case "6":
                    await ImportIntradaySymbolRangeAsync(host);
                    break;
                case "7":
                    await ViewStatisticsAsync(host);
                    break;
                case "8":
                    await ResumeIncompleteImportAsync(host);
                    break;
                case "9":
                    await CheckAndImportMissingSymbolsAsync(host, "D1");
                    break;
                case "10":
                    await CheckAndImportMissingSymbolsAsync(host, "M1");
                    break;
                case "0":
                    Log.Information("User requested exit");
                    return;
                default:
                    Console.WriteLine("❌ Invalid option");
                    break;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error processing option {Option}", option);
            Console.WriteLine($"❌ Error: {ex.Message}");
        }

        Console.WriteLine();
        Console.Write("Press any key to continue...");
        Console.ReadKey();
    }
}

static async Task ImportSingleSymbolAsync(IHost host)
{
    Console.WriteLine();
    Console.WriteLine("─────────────────────────────────────────────");
    Console.WriteLine("Import Daily (D1) Data for Specific Symbol");
    Console.WriteLine("─────────────────────────────────────────────");

    Console.Write("Enter symbol (e.g., FPT): ");
    var ticker = Console.ReadLine()?.Trim().ToUpper();
    if (string.IsNullOrEmpty(ticker))
    {
        Console.WriteLine("❌ Symbol is required");
        return;
    }

    Console.Write("From date (dd/MM/yyyy) [default: 5 years ago]: ");
    var fromDateStr = Console.ReadLine()?.Trim();
    var fromDate = string.IsNullOrEmpty(fromDateStr)
        ? DateTime.Now.AddYears(-5)
        : DateTime.ParseExact(fromDateStr, "dd/MM/yyyy", null);

    Console.Write("To date (dd/MM/yyyy) [default: today]: ");
    var toDateStr = Console.ReadLine()?.Trim();
    var toDate = string.IsNullOrEmpty(toDateStr)
        ? DateTime.Now
        : DateTime.ParseExact(toDateStr, "dd/MM/yyyy", null);

    using var scope = host.Services.CreateScope();
    var importer = scope.ServiceProvider.GetRequiredService<DailyOhlcvImporter>();

    Console.WriteLine();
    Console.WriteLine($"⏳ Importing data for {ticker} from {fromDate:dd/MM/yyyy} to {toDate:dd/MM/yyyy}...");

    var result = await importer.ImportAsync(ticker, fromDate, toDate);

    Console.WriteLine();
    if (!string.IsNullOrEmpty(result.ErrorMessage))
    {
        Console.WriteLine($"❌ Error: {result.ErrorMessage}");
    }
    else
    {
        Console.WriteLine($"✅ Success: {result.SuccessCount} records imported");
        if (result.FailedCount > 0)
            Console.WriteLine($"⚠️  Failed: {result.FailedCount} records");
    }
}

static async Task ImportAllSymbolsAsync(IHost host)
{
    Console.WriteLine();
    Console.WriteLine("─────────────────────────────────────────────");
    Console.WriteLine("Import Daily (D1) Data for ALL Symbols");
    Console.WriteLine("─────────────────────────────────────────────");

    // Check for existing progress
    var existingProgress = ImportProgress.LoadCurrent();
    if (existingProgress != null && (existingProgress.Status == "InProgress" || existingProgress.Status == "Cancelled"))
    {
        Console.WriteLine();
        Console.WriteLine($"📋 Found incomplete import job from {existingProgress.StartTime:dd/MM/yyyy HH:mm:ss}");
        Console.WriteLine($"   Progress: {existingProgress.ProcessedSymbols}/{existingProgress.TotalSymbols} symbols");
        Console.WriteLine($"   Remaining: {existingProgress.RemainingTickers.Count} symbols");
        Console.Write("Resume this job? (y/n): ");
        
        if (Console.ReadLine()?.Trim().ToLower() == "y")
        {
            await ResumeImportAsync(host, existingProgress);
            return;
        }
        else
        {
            ImportProgress.ClearCurrent();
        }
    }

    Console.Write("From date (dd/MM/yyyy) [default: 5 years ago]: ");
    var fromDateStr = Console.ReadLine()?.Trim();
    var fromDate = string.IsNullOrEmpty(fromDateStr)
        ? DateTime.Now.AddYears(-5)
        : DateTime.ParseExact(fromDateStr, "dd/MM/yyyy", null);

    Console.Write("To date (dd/MM/yyyy) [default: today]: ");
    var toDateStr = Console.ReadLine()?.Trim();
    var toDate = string.IsNullOrEmpty(toDateStr)
        ? DateTime.Now
        : DateTime.ParseExact(toDateStr, "dd/MM/yyyy", null);

    using var scope = host.Services.CreateScope();
    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    var importer = scope.ServiceProvider.GetRequiredService<DailyOhlcvImporter>();

    Console.WriteLine();
    Console.WriteLine("⏳ Loading symbol list from database...");
    
    var symbols = await uow.Symbols.GetAllAsync();
    var tickers = symbols.Select(s => s.Ticker).ToList();

    Console.WriteLine($"Found {tickers.Count} symbols");
    Console.Write($"⚠️  This will import data for ALL {tickers.Count} symbols. Continue? (y/n): ");
    
    if (Console.ReadLine()?.Trim().ToLower() != "y")
    {
        Console.WriteLine("❌ Import cancelled");
        return;
    }

    // Create progress tracker
    var progress = new ImportProgress
    {
        TotalSymbols = tickers.Count,
        RemainingTickers = new List<string>(tickers),
        FromDate = fromDate,
        ToDate = toDate
    };
    progress.Save();

    Console.WriteLine();
    Console.WriteLine($"⏳ Starting batch import for {tickers.Count} symbols from {fromDate:dd/MM/yyyy} to {toDate:dd/MM/yyyy}...");
    Console.WriteLine("💡 Press Ctrl+C to stop gracefully and save progress");

    try
    {
        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            Console.WriteLine("\n⚠️  Stopping... Saving progress...");
            progress.Cancel();
            cts.Cancel();
        };

        var batchResult = await importer.ImportBatchAsync(tickers, fromDate, toDate, progress, cts.Token);
        
        progress.Complete();
        
        PrintImportSummary(batchResult);
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("\n✅ Import stopped gracefully. Progress saved.");
        Console.WriteLine($"📊 Processed: {progress.ProcessedSymbols}/{progress.TotalSymbols} symbols");
        Console.WriteLine($"💾 Progress file: Progress/current_import.json");
    }
}

static async Task ResumeIncompleteImportAsync(IHost host)
{
    var progress = ImportProgress.LoadCurrent();
    if (progress == null || (progress.Status != "InProgress" && progress.Status != "Cancelled"))
    {
        Console.WriteLine("\n❌ No incomplete import found");
        return;
    }

    Console.WriteLine();
    Console.WriteLine($"📋 Found incomplete import job from {progress.StartTime:dd/MM/yyyy HH:mm:ss}");
    Console.WriteLine($"   Progress: {progress.ProcessedSymbols}/{progress.TotalSymbols} symbols");
    Console.WriteLine($"   Remaining: {progress.RemainingTickers.Count} symbols");
    Console.Write("Resume? (y/n): ");
    
    if (Console.ReadLine()?.Trim().ToLower() != "y")
    {
        Console.WriteLine("❌ Cancelled");
        return;
    }

    await ResumeImportAsync(host, progress);
}

static async Task ResumeImportAsync(IHost host, ImportProgress progress)
{
    using var scope = host.Services.CreateScope();
    var importer = scope.ServiceProvider.GetRequiredService<DailyOhlcvImporter>();

    Console.WriteLine();
    Console.WriteLine($"▶️  Resuming import for {progress.RemainingTickers.Count} remaining symbols...");
    Console.WriteLine("💡 Press Ctrl+C to stop gracefully and save progress");

    try
    {
        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            Console.WriteLine("\n⚠️  Stopping... Saving progress...");
            progress.Cancel();
            cts.Cancel();
        };

        var batchResult = await importer.ImportBatchAsync(
            progress.RemainingTickers, 
            progress.FromDate, 
            progress.ToDate,
            progress,
            cts.Token);
        
        progress.Complete();
        
        PrintImportSummary(batchResult);
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("\n✅ Import stopped gracefully. Progress saved.");
        Console.WriteLine($"📊 Processed: {progress.ProcessedSymbols}/{progress.TotalSymbols} symbols");
        Console.WriteLine($"💾 Progress file: Progress/current_import.json");
    }
}

static void PrintImportSummary(ImportBatchResult batchResult)
{
    Console.WriteLine();
    Console.WriteLine("════════════════════════════════════════");
    Console.WriteLine("Import Summary");
    Console.WriteLine("════════════════════════════════════════");
    Console.WriteLine($"✅ Total Success: {batchResult.TotalSuccess} records");
    Console.WriteLine($"❌ Total Failed: {batchResult.TotalFailed} records");
    Console.WriteLine($"📊 Symbols Processed: {batchResult.Results.Count}");
    
    var failedSymbols = batchResult.Results.Where(r => !string.IsNullOrEmpty(r.ErrorMessage)).ToList();
    if (failedSymbols.Count > 0)
    {
        Console.WriteLine();
        Console.WriteLine($"⚠️  Failed Symbols ({failedSymbols.Count}):");
        foreach (var failed in failedSymbols.Take(10))
        {
            Console.WriteLine($"   - {failed.Ticker}: {failed.ErrorMessage}");
        }
        if (failedSymbols.Count > 10)
            Console.WriteLine($"   ... and {failedSymbols.Count - 10} more");
    }
}

static async Task ImportSymbolRangeAsync(IHost host)
{
    Console.WriteLine();
    Console.WriteLine("─────────────────────────────────────────────");
    Console.WriteLine("Import Daily (D1) Data for Symbol Range");
    Console.WriteLine("─────────────────────────────────────────────");

    Console.Write("Enter symbols (comma-separated, e.g., FPT,VNM,HPG): ");
    var tickersInput = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(tickersInput))
    {
        Console.WriteLine("❌ Symbols are required");
        return;
    }

    var tickers = tickersInput.Split(',')
        .Select(t => t.Trim().ToUpper())
        .Where(t => !string.IsNullOrEmpty(t))
        .ToList();

    if (tickers.Count == 0)
    {
        Console.WriteLine("❌ No valid symbols provided");
        return;
    }

    Console.Write("From date (dd/MM/yyyy) [default: 5 years ago]: ");
    var fromDateStr = Console.ReadLine()?.Trim();
    var fromDate = string.IsNullOrEmpty(fromDateStr)
        ? DateTime.Now.AddYears(-5)
        : DateTime.ParseExact(fromDateStr, "dd/MM/yyyy", null);

    Console.Write("To date (dd/MM/yyyy) [default: today]: ");
    var toDateStr = Console.ReadLine()?.Trim();
    var toDate = string.IsNullOrEmpty(toDateStr)
        ? DateTime.Now
        : DateTime.ParseExact(toDateStr, "dd/MM/yyyy", null);

    using var scope = host.Services.CreateScope();
    var importer = scope.ServiceProvider.GetRequiredService<DailyOhlcvImporter>();

    Console.WriteLine();
    Console.WriteLine($"⏳ Starting import for {tickers.Count} symbols from {fromDate:dd/MM/yyyy} to {toDate:dd/MM/yyyy}...");

    var batchResult = await importer.ImportBatchAsync(tickers, fromDate, toDate);

    Console.WriteLine();
    Console.WriteLine("════════════════════════════════════════");
    Console.WriteLine("Import Summary");
    Console.WriteLine("════════════════════════════════════════");
    Console.WriteLine($"✅ Total Success: {batchResult.TotalSuccess} records");
    Console.WriteLine($"❌ Total Failed: {batchResult.TotalFailed} records");
    Console.WriteLine($"📊 Symbols Processed: {batchResult.Results.Count}");
    
    Console.WriteLine();
    Console.WriteLine("Details:");
    foreach (var result in batchResult.Results)
    {
        if (!string.IsNullOrEmpty(result.ErrorMessage))
            Console.WriteLine($"   ❌ {result.Ticker}: {result.ErrorMessage}");
        else
            Console.WriteLine($"   ✅ {result.Ticker}: {result.SuccessCount} records");
    }
}

static async Task ImportSingleIntradaySymbolAsync(IHost host)
{
    Console.WriteLine();
    Console.WriteLine("─────────────────────────────────────────────");
    Console.WriteLine("Import Intraday (M1) Data for Specific Symbol");
    Console.WriteLine("─────────────────────────────────────────────");

    Console.Write("Enter symbol (e.g., FPT): ");
    var ticker = Console.ReadLine()?.Trim().ToUpper();
    if (string.IsNullOrEmpty(ticker))
    {
        Console.WriteLine("❌ Symbol is required");
        return;
    }

    Console.Write("From date (dd/MM/yyyy) [default: 6 months ago]: ");
    var fromDateStr = Console.ReadLine()?.Trim();
    var fromDate = string.IsNullOrEmpty(fromDateStr)
        ? DateTime.Now.AddMonths(-6)
        : DateTime.ParseExact(fromDateStr, "dd/MM/yyyy", null);

    Console.Write("To date (dd/MM/yyyy) [default: today]: ");
    var toDateStr = Console.ReadLine()?.Trim();
    var toDate = string.IsNullOrEmpty(toDateStr)
        ? DateTime.Now
        : DateTime.ParseExact(toDateStr, "dd/MM/yyyy", null);

    using var scope = host.Services.CreateScope();
    var importer = scope.ServiceProvider.GetRequiredService<IntradayOhlcvImporter>();

    Console.WriteLine();
    Console.WriteLine($"⏳ Importing data for {ticker} from {fromDate:dd/MM/yyyy} to {toDate:dd/MM/yyyy}...");

    var result = await importer.ImportAsync(ticker, fromDate, toDate);

    Console.WriteLine();
    if (!string.IsNullOrEmpty(result.ErrorMessage))
    {
        Console.WriteLine($"❌ Error: {result.ErrorMessage}");
    }
    else
    {
        Console.WriteLine($"✅ Success: {result.SuccessCount} records imported");
        if (result.FailedCount > 0)
            Console.WriteLine($"⚠️  Failed: {result.FailedCount} records");
    }
}

static async Task ImportAllIntradaySymbolsAsync(IHost host)
{
    Console.WriteLine();
    Console.WriteLine("─────────────────────────────────────────────");
    Console.WriteLine("Import Intraday (M1) Data for ALL Symbols");
    Console.WriteLine("─────────────────────────────────────────────");

    // Check for existing progress
    var existingProgress = ImportProgress.LoadCurrent();
    if (existingProgress != null && (existingProgress.Status == "InProgress" || existingProgress.Status == "Cancelled"))
    {
        Console.WriteLine();
        Console.WriteLine($"📋 Found incomplete import job from {existingProgress.StartTime:dd/MM/yyyy HH:mm:ss}");
        Console.WriteLine($"   Progress: {existingProgress.ProcessedSymbols}/{existingProgress.TotalSymbols} symbols");
        Console.WriteLine($"   Remaining: {existingProgress.RemainingTickers.Count} symbols");
        Console.Write("Resume this job? (y/n): ");
        
        if (Console.ReadLine()?.Trim().ToLower() == "y")
        {
            await ResumeIntradayImportAsync(host, existingProgress);
            return;
        }
        else
        {
            ImportProgress.ClearCurrent();
        }
    }

    Console.Write("From date (dd/MM/yyyy) [default: 6 months ago]: ");
    var fromDateStr = Console.ReadLine()?.Trim();
    var fromDate = string.IsNullOrEmpty(fromDateStr)
        ? DateTime.Now.AddMonths(-6)
        : DateTime.ParseExact(fromDateStr, "dd/MM/yyyy", null);

    Console.Write("To date (dd/MM/yyyy) [default: today]: ");
    var toDateStr = Console.ReadLine()?.Trim();
    var toDate = string.IsNullOrEmpty(toDateStr)
        ? DateTime.Now
        : DateTime.ParseExact(toDateStr, "dd/MM/yyyy", null);

    using var scope = host.Services.CreateScope();
    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    var importer = scope.ServiceProvider.GetRequiredService<IntradayOhlcvImporter>();

    var symbols = await uow.Symbols.GetAllAsync();
    var tickers = symbols.Select(s => s.Ticker).ToList();

    // Setup cancellation token
    using var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (s, e) =>
    {
        e.Cancel = true;
        Console.WriteLine();
        Console.WriteLine("⚠️  Cancellation requested. Finishing current symbol and saving progress...");
        cts.Cancel();
    };

    // Create progress tracker
    var progress = new ImportProgress
    {
        TotalSymbols = tickers.Count,
        RemainingTickers = new List<string>(tickers),
        StartTime = DateTime.Now,
        FromDate = fromDate,
        ToDate = toDate,
        Timeframe = "M1"
    };
    progress.Save();

    Console.WriteLine();
    Console.WriteLine($"⏳ Starting import for {tickers.Count} symbols from {fromDate:dd/MM/yyyy} to {toDate:dd/MM/yyyy}...");

    var batchResult = await importer.ImportBatchAsync(tickers, fromDate, toDate, progress, cts.Token);

    // Mark as completed
    progress.Complete();

    Console.WriteLine();
    Console.WriteLine("════════════════════════════════════════");
    Console.WriteLine("Import Summary");
    Console.WriteLine("════════════════════════════════════════");
    Console.WriteLine($"✅ Total Success: {batchResult.TotalSuccess} records");
    Console.WriteLine($"❌ Total Failed: {batchResult.TotalFailed} records");
    Console.WriteLine($"📊 Symbols Processed: {batchResult.Results.Count}");
    Console.WriteLine($"✅ Successful Symbols: {progress.SuccessSymbols}");
    Console.WriteLine($"❌ Failed Symbols: {progress.FailedSymbols}");
}

static async Task ImportIntradaySymbolRangeAsync(IHost host)
{
    Console.WriteLine();
    Console.WriteLine("─────────────────────────────────────────────");
    Console.WriteLine("Import Intraday (M1) Data for Symbol Range");
    Console.WriteLine("─────────────────────────────────────────────");

    Console.Write("Enter symbols (comma-separated, e.g., FPT,VNM,HPG): ");
    var tickersInput = Console.ReadLine()?.Trim();
    if (string.IsNullOrEmpty(tickersInput))
    {
        Console.WriteLine("❌ Symbols are required");
        return;
    }

    var tickers = tickersInput.Split(',')
        .Select(t => t.Trim().ToUpper())
        .Where(t => !string.IsNullOrEmpty(t))
        .ToList();

    if (tickers.Count == 0)
    {
        Console.WriteLine("❌ No valid symbols provided");
        return;
    }

    Console.Write("From date (dd/MM/yyyy) [default: 6 months ago]: ");
    var fromDateStr = Console.ReadLine()?.Trim();
    var fromDate = string.IsNullOrEmpty(fromDateStr)
        ? DateTime.Now.AddMonths(-6)
        : DateTime.ParseExact(fromDateStr, "dd/MM/yyyy", null);

    Console.Write("To date (dd/MM/yyyy) [default: today]: ");
    var toDateStr = Console.ReadLine()?.Trim();
    var toDate = string.IsNullOrEmpty(toDateStr)
        ? DateTime.Now
        : DateTime.ParseExact(toDateStr, "dd/MM/yyyy", null);

    using var scope = host.Services.CreateScope();
    var importer = scope.ServiceProvider.GetRequiredService<IntradayOhlcvImporter>();

    Console.WriteLine();
    Console.WriteLine($"⏳ Starting import for {tickers.Count} symbols from {fromDate:dd/MM/yyyy} to {toDate:dd/MM/yyyy}...");

    var batchResult = await importer.ImportBatchAsync(tickers, fromDate, toDate);

    Console.WriteLine();
    Console.WriteLine("════════════════════════════════════════");
    Console.WriteLine("Import Summary");
    Console.WriteLine("════════════════════════════════════════");
    Console.WriteLine($"✅ Total Success: {batchResult.TotalSuccess} records");
    Console.WriteLine($"❌ Total Failed: {batchResult.TotalFailed} records");
    Console.WriteLine($"📊 Symbols Processed: {batchResult.Results.Count}");
    
    Console.WriteLine();
    Console.WriteLine("Details:");
    foreach (var result in batchResult.Results)
    {
        if (!string.IsNullOrEmpty(result.ErrorMessage))
            Console.WriteLine($"   ❌ {result.Ticker}: {result.ErrorMessage}");
        else
            Console.WriteLine($"   ✅ {result.Ticker}: {result.SuccessCount} records");
    }
}

static async Task ResumeIntradayImportAsync(IHost host, ImportProgress progress)
{
    using var scope = host.Services.CreateScope();
    var importer = scope.ServiceProvider.GetRequiredService<IntradayOhlcvImporter>();

    // Setup cancellation token
    using var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (s, e) =>
    {
        e.Cancel = true;
        Console.WriteLine();
        Console.WriteLine("⚠️  Cancellation requested. Finishing current symbol and saving progress...");
        cts.Cancel();
    };

    Console.WriteLine();
    Console.WriteLine($"⏳ Resuming import for {progress.RemainingTickers.Count} remaining symbols...");

    var batchResult = await importer.ImportBatchAsync(
        progress.RemainingTickers, 
        progress.FromDate, 
        progress.ToDate, 
        progress, 
        cts.Token);

    if (progress.RemainingTickers.Count == 0)
    {
        progress.Complete();
    }

    Console.WriteLine();
    Console.WriteLine("════════════════════════════════════════");
    Console.WriteLine("Resume Summary");
    Console.WriteLine("════════════════════════════════════════");
    Console.WriteLine($"✅ Total Success: {batchResult.TotalSuccess} records");
    Console.WriteLine($"❌ Total Failed: {batchResult.TotalFailed} records");
    Console.WriteLine($"📊 Overall Progress: {progress.ProcessedSymbols}/{progress.TotalSymbols} symbols");
    Console.WriteLine($"✅ Successful Symbols: {progress.SuccessSymbols}");
    Console.WriteLine($"❌ Failed Symbols: {progress.FailedSymbols}");
}

static async Task ViewStatisticsAsync(IHost host)
{
    Console.WriteLine();
    Console.WriteLine("─────────────────────────────────────────────");
    Console.WriteLine("OHLCV Data Statistics");
    Console.WriteLine("─────────────────────────────────────────────");

    using var scope = host.Services.CreateScope();
    var ohlcvUow = scope.ServiceProvider.GetRequiredService<IOhlcvUnitOfWork>();

    try
    {
        var allData = await ohlcvUow.Ohlcv.GetByTickerAndTimeRangeAsync(
            "", 
            "", 
            DateTime.MinValue, 
            DateTime.MaxValue, 
            default);
        var d1Data = allData.Where(o => o.Timeframe == "D1").ToList();
        
        Console.WriteLine($"Total OHLCV records: {allData.Count}");
        Console.WriteLine($"D1 (Daily) records: {d1Data.Count}");
        Console.WriteLine($"Unique symbols: {d1Data.Select(o => o.Ticker).Distinct().Count()}");
        
        if (d1Data.Count > 0)
        {
            var oldestDate = d1Data.Min(o => o.Time);
            var newestDate = d1Data.Max(o => o.Time);
            Console.WriteLine($"Date range: {oldestDate:dd/MM/yyyy} to {newestDate:dd/MM/yyyy}");

            var topSymbols = d1Data.GroupBy(o => o.Ticker)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .ToList();

            Console.WriteLine();
            Console.WriteLine("Top 10 symbols by record count:");
            foreach (var group in topSymbols)
            {
                Console.WriteLine($"   {group.Key}: {group.Count()} records");
            }
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error retrieving statistics");
        Console.WriteLine($"❌ Error: {ex.Message}");
    }
}

static async Task RunAutomaticModeAsync(IHost host, string[] args)
{
    Log.Information("Running in automatic mode with arguments: {Args}", string.Join(" ", args));

    if (args.Length < 2)
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run -- import-all <fromDate> <toDate>");
        Console.WriteLine("  dotnet run -- import-symbol <ticker> <fromDate> <toDate>");
        Console.WriteLine();
        Console.WriteLine("Date format: dd/MM/yyyy");
        return;
    }

    var command = args[0].ToLower();

    using var scope = host.Services.CreateScope();
    var importer = scope.ServiceProvider.GetRequiredService<DailyOhlcvImporter>();

    if (command == "import-all" && args.Length == 3)
    {
        var fromDate = DateTime.ParseExact(args[1], "dd/MM/yyyy", null);
        var toDate = DateTime.ParseExact(args[2], "dd/MM/yyyy", null);

        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var symbols = await uow.Symbols.GetAllAsync();
        var tickers = symbols.Select(s => s.Ticker).ToList();

        Log.Information("Starting automatic batch import for {Count} symbols", tickers.Count);
        await importer.ImportBatchAsync(tickers, fromDate, toDate);
    }
    else if (command == "import-symbol" && args.Length == 4)
    {
        var ticker = args[1].ToUpper();
        var fromDate = DateTime.ParseExact(args[2], "dd/MM/yyyy", null);
        var toDate = DateTime.ParseExact(args[3], "dd/MM/yyyy", null);

        Log.Information("Starting automatic import for {Ticker}", ticker);
        await importer.ImportAsync(ticker, fromDate, toDate);
    }
    else
    {
        Console.WriteLine("Invalid command or arguments");
    }
}

static async Task CheckAndImportMissingSymbolsAsync(IHost host, string timeframe)
{
    Console.WriteLine();
    Console.WriteLine("─────────────────────────────────────────────");
    Console.WriteLine($"Check & Import Missing Symbols ({timeframe})");
    Console.WriteLine("─────────────────────────────────────────────");

    using var scope = host.Services.CreateScope();
    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    var ohlcvUow = scope.ServiceProvider.GetRequiredService<IOhlcvUnitOfWork>();

    try
    {
        // 1. Lấy tất cả symbols từ GDT DB
        Console.WriteLine("📊 Fetching all symbols from database...");
        var allSymbols = await uow.Symbols.GetAllAsync();
        var allTickers = allSymbols.Select(s => s.Ticker).ToHashSet();
        Console.WriteLine($"   Found {allTickers.Count} symbols in GreenDragonTrading DB");

        // 2. Lấy danh sách tickers đã có data trong OHLCV DB
        Console.WriteLine($"📊 Checking existing {timeframe} data in OHLCV DB...");
        var existingTickers = await ohlcvUow.Ohlcv.GetAvailableTickersAsync(timeframe);
        Console.WriteLine($"   Found {existingTickers.Count} symbols with {timeframe} data");

        // 3. Tìm missing tickers
        var missingTickers = allTickers.Except(existingTickers).OrderBy(t => t).ToList();

        if (missingTickers.Count == 0)
        {
            Console.WriteLine("\n✅ All symbols have data! No missing symbols.");
            return;
        }

        // 4. Hiển thị missing symbols
        Console.WriteLine($"\n⚠️  Found {missingTickers.Count} missing symbols:");
        Console.WriteLine("─────────────────────────────────────────────");
        
        var displayCount = Math.Min(50, missingTickers.Count);
        foreach (var ticker in missingTickers.Take(displayCount))
        {
            Console.Write($"{ticker} ");
        }
        if (missingTickers.Count > displayCount)
        {
            Console.Write($"... and {missingTickers.Count - displayCount} more");
        }
        Console.WriteLine("\n─────────────────────────────────────────────");

        // 5. Confirm import
        Console.WriteLine($"\n📥 Import {timeframe} data for these {missingTickers.Count} missing symbols?");
        Console.Write("Continue? (y/n): ");
        
        if (Console.ReadLine()?.Trim().ToLower() != "y")
        {
            Console.WriteLine("❌ Cancelled");
            return;
        }

        // 6. Get date range
        DateTime fromDate, toDate;
        
        if (timeframe == "D1")
        {
            Console.Write($"\nFrom date (dd/MM/yyyy) [default: 5 years ago]: ");
            var fromDateStr = Console.ReadLine()?.Trim();
            fromDate = string.IsNullOrEmpty(fromDateStr)
                ? DateTime.Now.AddYears(-5)
                : DateTime.ParseExact(fromDateStr, "dd/MM/yyyy", null);
        }
        else // M1
        {
            Console.Write($"\nFrom date (dd/MM/yyyy) [default: 6 months ago]: ");
            var fromDateStr = Console.ReadLine()?.Trim();
            fromDate = string.IsNullOrEmpty(fromDateStr)
                ? DateTime.Now.AddMonths(-6)
                : DateTime.ParseExact(fromDateStr, "dd/MM/yyyy", null);
        }

        Console.Write("To date (dd/MM/yyyy) [default: today]: ");
        var toDateStr = Console.ReadLine()?.Trim();
        toDate = string.IsNullOrEmpty(toDateStr)
            ? DateTime.Now
            : DateTime.ParseExact(toDateStr, "dd/MM/yyyy", null);

        // 7. Import missing symbols
        Console.WriteLine($"\n⏳ Starting import for {missingTickers.Count} missing symbols from {fromDate:dd/MM/yyyy} to {toDate:dd/MM/yyyy}...");
        Console.WriteLine("💡 Press Ctrl+C to stop gracefully and save progress");

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
        {
            e.Cancel = true;
            Console.WriteLine("\n⚠️  Cancellation requested. Finishing current symbol...");
            cts.Cancel();
        };

        if (timeframe == "D1")
        {
            var importer = scope.ServiceProvider.GetRequiredService<DailyOhlcvImporter>();
            
            var progress = new ImportProgress
            {
                TotalSymbols = missingTickers.Count,
                RemainingTickers = new List<string>(missingTickers),
                StartTime = DateTime.Now,
                FromDate = fromDate,
                ToDate = toDate,
                Timeframe = "D1"
            };
            progress.Save();

            var result = await importer.ImportBatchAsync(missingTickers, fromDate, toDate, progress, cts.Token);

            progress.Complete();

            Console.WriteLine("\n════════════════════════════════════════");
            Console.WriteLine("Import Summary");
            Console.WriteLine("════════════════════════════════════════");
            Console.WriteLine($"✅ Total Success: {result.TotalSuccess} records");
            Console.WriteLine($"❌ Total Failed: {result.TotalFailed} records");
            Console.WriteLine($"📊 Symbols Processed: {result.Results.Count}");
            Console.WriteLine($"✅ Successful Symbols: {progress.SuccessSymbols}");
            Console.WriteLine($"❌ Failed Symbols: {progress.FailedSymbols}");
        }
        else // M1
        {
            var importer = scope.ServiceProvider.GetRequiredService<IntradayOhlcvImporter>();
            
            var progress = new ImportProgress
            {
                TotalSymbols = missingTickers.Count,
                RemainingTickers = new List<string>(missingTickers),
                StartTime = DateTime.Now,
                FromDate = fromDate,
                ToDate = toDate,
                Timeframe = "M1"
            };
            progress.Save();

            var result = await importer.ImportBatchAsync(missingTickers, fromDate, toDate, progress, cts.Token);

            progress.Complete();

            Console.WriteLine("\n════════════════════════════════════════");
            Console.WriteLine("Import Summary");
            Console.WriteLine("════════════════════════════════════════");
            Console.WriteLine($"✅ Total Success: {result.TotalSuccess} records");
            Console.WriteLine($"❌ Total Failed: {result.TotalFailed} records");
            Console.WriteLine($"📊 Symbols Processed: {result.Results.Count}");
            Console.WriteLine($"✅ Successful Symbols: {progress.SuccessSymbols}");
            Console.WriteLine($"❌ Failed Symbols: {progress.FailedSymbols}");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Error checking missing symbols");
        Console.WriteLine($"\n❌ Error: {ex.Message}");
    }
}
