# Script to run both API Server and SignalR Client

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Starting API Server + SignalR Client" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Start API Server in new window
Write-Host "Starting API Server..." -ForegroundColor Green
$apiPath = Join-Path $PSScriptRoot "GreenDragonTrading.Api"
$apiCommand = "cd '$apiPath'; Write-Host '=== API SERVER ===' -ForegroundColor Cyan; dotnet run"
$apiProcess = Start-Process powershell -ArgumentList "-NoExit", "-Command", $apiCommand -PassThru

Write-Host "   API Server started in new window (PID: $($apiProcess.Id))" -ForegroundColor Gray
Write-Host ""

# Wait for API to start
Write-Host "Waiting for API to start (10 seconds)..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Check if API is running
$apiRunning = $false
$attempts = 0
while (-not $apiRunning -and $attempts -lt 5) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5146" -Method Head -TimeoutSec 2 -ErrorAction SilentlyContinue
        $apiRunning = $true
        Write-Host "API is running!" -ForegroundColor Green
    } catch {
        $attempts++
        Write-Host "   Attempt $attempts/5: API not ready yet..." -ForegroundColor Gray
        Start-Sleep -Seconds 2
    }
}

if (-not $apiRunning) {
    Write-Host "WARNING: Could not verify API is running" -ForegroundColor Yellow
    Write-Host "   Proceeding anyway..." -ForegroundColor Yellow
}

Write-Host ""

# Start SignalR Client in new window
Write-Host "Starting SignalR Client..." -ForegroundColor Green
$clientPath = Join-Path $PSScriptRoot "GreenDragonTrading.SignalRClient"
$clientCommand = "cd '$clientPath'; Write-Host '=== SIGNALR CLIENT ===' -ForegroundColor Cyan; dotnet run"
$clientProcess = Start-Process powershell -ArgumentList "-NoExit", "-Command", $clientCommand -PassThru

Write-Host "   SignalR Client started in new window (PID: $($clientProcess.Id))" -ForegroundColor Gray
Write-Host ""

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Both API Server and Client are running!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Logs location:" -ForegroundColor Yellow
Write-Host "   API Server: GreenDragonTrading.Api/Logs/log-*.txt" -ForegroundColor Gray
Write-Host "   Client:     GreenDragonTrading.SignalRClient/Logs/signalr-client-*.txt" -ForegroundColor Gray
Write-Host ""
Write-Host "To stop:" -ForegroundColor Yellow
Write-Host "   - Press Ctrl+C in each window" -ForegroundColor Gray
Write-Host "   - Or close the windows" -ForegroundColor Gray
Write-Host ""
Write-Host "Press any key to exit this window (processes will keep running)..." -ForegroundColor Cyan
$null = $Host.UI.RawUI.ReadKey('NoEcho,IncludeKeyDown')
