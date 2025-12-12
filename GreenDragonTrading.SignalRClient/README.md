# GreenDragonTrading SignalR Client

Console application để test và monitor real-time market data từ MarketDataHub.

## Chức năng

- Kết nối đến MarketDataHub qua SignalR
- Subscribe vào các symbol cụ thể
- Nhận và log tất cả market data real-time
- Sử dụng Serilog y chang bên API để dễ so sánh logs
- Auto-reconnect khi mất kết nối

## Chạy Client

### Cách 1: Chạy với default settings
```powershell
dotnet run
```

Default:
- Hub URL: `http://localhost:5146/hubs/marketdata`
- Symbols: `FPT,SSI,VNM,HPG,VCB`

### Cách 2: Custom hub URL
```powershell
dotnet run http://localhost:5146/hubs/marketdata
```

### Cách 3: Custom hub URL + symbols
```powershell
dotnet run http://localhost:5146/hubs/marketdata "FPT,SSI,VNM"
```

### Cách 4: Treo chạy nền (Windows)
```powershell
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PWD'; dotnet run"
```

## Logs

Client sẽ ghi log vào 2 nơi:
1. **Console**: Hiển thị trực tiếp
2. **File**: `Logs/signalr-client-YYYYMMDD.txt`

Format log giống y chang bên API để dễ so sánh.

## So sánh logs Server vs Client

### Server logs (API):
```
GreenDragonTrading.Api/Logs/log-YYYYMMDD.txt
```

### Client logs:
```
GreenDragonTrading.SignalRClient/Logs/signalr-client-YYYYMMDD.txt
```

Mở 2 file này song song để xem:
- Server broadcast data gì
- Client nhận được data gì
- Có mất data không
- Delay bao nhiêu

## Ví dụ Output

```
[02:00:15 INF] ================================================================================
[02:00:15 INF] Starting GreenDragonTrading SignalR Client
[02:00:15 INF] ================================================================================
[02:00:15 INF] Hub URL: http://localhost:5146/hubs/marketdata
[02:00:15 INF] Subscribing to symbols: FPT, SSI, VNM, HPG, VCB
[02:00:15 INF] ✅ Handlers registered
[02:00:15 INF] Connecting to MarketDataHub...
[02:00:15 INF] ✅ Connected successfully! ConnectionId: abc123
[02:00:15 INF] Subscribing to symbols...
[02:00:15 INF] ✅ Subscribed successfully!
[02:00:15 INF]
[02:00:15 INF] 📊 Client is running. Press Ctrl+C to stop...
[02:00:15 INF]
[02:00:16 INF] 📩 [SINGLE] Received market data for FPT
[02:00:17 INF] 📩 [SINGLE] Received market data for SSI
[02:00:18 INF] 📩 [SINGLE] Received market data for VNM
[02:00:25 INF] 📈 Stats: Messages=15, Unique Symbols=5
```

## Dừng Client

Nhấn `Ctrl + C` để dừng. Client sẽ:
1. Disconnect từ hub gracefully
2. Hiển thị final stats
3. Flush tất cả logs
4. Thoát

## Troubleshooting

### Lỗi kết nối
```
Error: Failed to connect
```
→ Kiểm tra API có đang chạy không (`dotnet run --project GreenDragonTrading.Api`)

### Không nhận data
```
Connected but no data received
```
→ Kiểm tra symbols có đúng không và có data thật từ SSI không
