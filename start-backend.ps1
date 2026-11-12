# Starts every backend service in its own PowerShell window.
#
# Usage (from the project root):
#   powershell -ExecutionPolicy Bypass -File .\start-backend.ps1        # browser on this computer only
#   powershell -ExecutionPolicy Bypass -File .\start-backend.ps1 -Lan   # also reachable from a phone on the same Wi-Fi
#
# Docker Desktop (Postgres + RabbitMQ) must already be running.

param([switch]$Lan)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path

$services = @(
    @{ Name = "Identity";     Path = "services/identity/Identity.Api";         Port = 5101 },
    @{ Name = "Garden";       Path = "services/garden/Garden.Api";             Port = 5103 },
    @{ Name = "Reminder";     Path = "services/reminder/Reminder.Api";         Port = 5104 },
    @{ Name = "Notification"; Path = "services/notification/Notification.Api"; Port = 5105 },
    @{ Name = "AI Assistant"; Path = "services/ai-assistant/AiAssistant.Api";  Port = 5106 },
    @{ Name = "Forum";        Path = "services/forum/Forum.Api";               Port = 5107 },
    @{ Name = "Sensor";       Path = "services/sensor/Sensor.Api";             Port = 5182 }
)

# Build once up front. Starting several "dotnet run" at the same time would make them
# fight over the shared Contracts project and fail with "file is being used by another process".
Write-Host "Building the solution..." -ForegroundColor Cyan
dotnet build (Join-Path $root "MamXanh.slnx") -nologo -v q
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed. Fix the errors above, then run this script again." -ForegroundColor Red
    exit 1
}

$bindAddress = if ($Lan) { "0.0.0.0" } else { "localhost" }

foreach ($s in $services) {
    $dir = Join-Path $root $s.Path
    $url = "http://$($bindAddress):$($s.Port)"
    $title = "$($s.Name) :$($s.Port)"
    $command = "`$Host.UI.RawUI.WindowTitle = '$title'; Set-Location '$dir'; dotnet run --no-build --urls $url"
    Start-Process powershell -ArgumentList "-NoExit", "-Command", $command
    Write-Host "Started $title" -ForegroundColor Green
}

Write-Host ""
Write-Host "All services are starting in separate windows. Close a window (or press Ctrl+C in it) to stop that service." -ForegroundColor Cyan
if ($Lan) {
    Write-Host "Phone mode: put this computer's Wi-Fi IP in frontend/mobile/.env, then run 'npx expo start -c'." -ForegroundColor Yellow
}
