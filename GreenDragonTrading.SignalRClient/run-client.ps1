# Script để chạy SignalR Client

Write-Host "================================" -ForegroundColor Cyan
Write-Host "GreenDragonTrading SignalR Client" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Check if API is running
$apiRunning = $false
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5146" -Method Head -TimeoutSec 2 -ErrorAction SilentlyContinue
    $apiRunning = $true
} catch {
    $apiRunning = $false
}

if (-not $apiRunning) {
    Write-Host "⚠️  WARNING: API seems not running on http://localhost:5146" -ForegroundColor Yellow
    Write-Host "   Please start the API first with: dotnet run --project ..\GreenDragonTrading.Api" -ForegroundColor Yellow
    Write-Host ""
    
    $continue = Read-Host "Do you want to continue anyway? (y/n)"
    if ($continue -ne 'y' -and $continue -ne 'Y') {
        Write-Host "Exiting..." -ForegroundColor Red
        exit 1
    }
    Write-Host ""
}

Write-Host "✅ Starting SignalR Client..." -ForegroundColor Green
Write-Host ""
Write-Host "Default configuration:" -ForegroundColor Gray
Write-Host "  - Hub URL: http://localhost:5146/hubs/marketdata" -ForegroundColor Gray
Write-Host "  - Symbols: FPT,SSI,VNM,HPG,VCB" -ForegroundColor Gray
Write-Host "  - Logs: Logs/signalr-client-*.txt" -ForegroundColor Gray
Write-Host ""
Write-Host "Press Ctrl+C to stop" -ForegroundColor Yellow
Write-Host ""

# Run the client
dotnet run
