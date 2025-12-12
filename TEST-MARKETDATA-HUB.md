# 🧪 Test MarketDataHub - Server vs Client Logs

Tool để test và monitor real-time market data broadcasting từ MarketDataHub.

## 📦 Cấu trúc

```
GreenDragonTrading.dotnet/
├── GreenDragonTrading.Api/              # API Server (Hub)
│   └── Logs/log-*.txt                   # Server logs
├── GreenDragonTrading.SignalRClient/    # SignalR Client
│   └── Logs/signalr-client-*.txt        # Client logs
└── run-server-and-client.ps1            # Script chạy cả 2
```

## 🚀 Cách sử dụng

### Option 1: Chạy tự động (Recommended)

```powershell
# Chạy cả Server và Client tự động
.\run-server-and-client.ps1
```

Script sẽ:
1. Mở API Server trong window mới
2. Đợi 10s cho server khởi động
3. Mở SignalR Client trong window mới
4. Cả 2 sẽ chạy song song

### Option 2: Chạy thủ công

**Terminal 1 - API Server:**
```powershell
cd GreenDragonTrading.Api
dotnet run
```

**Terminal 2 - SignalR Client:**
```powershell
cd GreenDragonTrading.SignalRClient
dotnet run
```

### Option 3: Chạy Client với custom config

```powershell
cd GreenDragonTrading.SignalRClient

# Custom symbols
dotnet run http://localhost:5146/hubs/marketdata "FPT,SSI,VNM"

# Hoặc dùng script
.\run-client.ps1
```

## 📊 So sánh Logs

### Server logs
```
GreenDragonTrading.Api/Logs/log-20251212.txt
```

Chứa:
- SSI streaming data received
- Market data broadcasted to clients
- Hub connection events

### Client logs
```
GreenDragonTrading.SignalRClient/Logs/signalr-client-20251212.txt
```

Chứa:
- Market data received from hub
- Connection events
- Statistics (message count, symbols)

### Mở cả 2 file để so sánh:

```powershell
# Mở cả 2 logs trong VS Code
code GreenDragonTrading.Api\Logs\log-*.txt GreenDragonTrading.SignalRClient\Logs\signalr-client-*.txt
```

Hoặc dùng PowerShell:
```powershell
# Tail logs real-time
Get-Content GreenDragonTrading.Api\Logs\log-*.txt -Wait -Tail 20
Get-Content GreenDragonTrading.SignalRClient\Logs\signalr-client-*.txt -Wait -Tail 20
```

## 🔍 Những gì cần kiểm tra

### 1. Data Flow
- ✅ Server nhận data từ SSI
- ✅ Server broadcast data qua Hub
- ✅ Client nhận data từ Hub
- ✅ Client log đúng format

### 2. Timing
- So sánh timestamp giữa server và client
- Kiểm tra delay giữa broadcast và receive
- Verify không mất data

### 3. Connection Stability
- Auto-reconnect khi mất kết nối
- Handle network issues
- Graceful shutdown

## 📝 Format Log

Cả 2 đều dùng Serilog với format:
```
[HH:mm:ss LEVEL] [SourceContext] Message
```

Ví dụ:
```
[02:15:30 INF] [GreenDragonTrading.Infrastructure.BackgroundWorkers.SsiStreamingBackgroundService] Received X-TRADE: {"Symbol":"FPT"...}
[02:15:30 INF] [] 📩 [SINGLE] Received market data for FPT
```

## 🛑 Dừng Test

**Nếu dùng script:**
- Close các PowerShell windows
- Hoặc Ctrl+C trong mỗi window

**Nếu chạy thủ công:**
- Nhấn Ctrl+C trong mỗi terminal

## 💡 Tips

### 1. Realtime monitoring
```powershell
# Terminal 1: Server logs
Get-Content GreenDragonTrading.Api\Logs\log-*.txt -Wait -Tail 10

# Terminal 2: Client logs  
Get-Content GreenDragonTrading.SignalRClient\Logs\signalr-client-*.txt -Wait -Tail 10
```

### 2. Filter logs
```powershell
# Chỉ xem market data
Get-Content .\Logs\*.txt | Select-String "Received.*FPT"

# Xem connection events
Get-Content .\Logs\*.txt | Select-String "Connected|Disconnected"
```

### 3. Count messages
```powershell
# Đếm số messages client nhận được
(Get-Content GreenDragonTrading.SignalRClient\Logs\signalr-client-*.txt | Select-String "Received market data").Count
```

## 🎯 Mục đích

Dùng tool này để:
- ✅ Verify Hub broadcast đúng data
- ✅ Verify Client nhận đủ data
- ✅ Debug timing issues
- ✅ Monitor real-time performance
- ✅ Test reconnection logic
- ✅ Compare server-side vs client-side views

## 🐛 Troubleshooting

### Client không kết nối được
```
Error: Failed to connect to http://localhost:5146/hubs/marketdata
```
→ Kiểm tra API có chạy không

### Client không nhận data
```
Connected but no data received
```
→ Kiểm tra:
1. Có subscribe symbols chưa?
2. Symbols có đúng không?
3. SSI có đang gửi data không?

### Logs không ra file
```
No log files created
```
→ Kiểm tra folder `Logs/` có tồn tại không (tự động tạo)

## 📚 Tham khảo

- [SignalR Client Docs](https://learn.microsoft.com/en-us/aspnet/core/signalr/dotnet-client)
- [Serilog Docs](https://serilog.net/)
- Project README: [GreenDragonTrading.SignalRClient/README.md](GreenDragonTrading.SignalRClient/README.md)
