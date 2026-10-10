$process = Start-Process -FilePath "dotnet" -ArgumentList "C:\Projects\BusBookingApi\src\BusBooking.Api\bin\Debug\net10.0\BusBooking.Api.dll --urls http://localhost:5241" -PassThru
Start-Sleep -Seconds 3

try {
    $res = Invoke-RestMethod -Uri "http://localhost:5241/health/live" -TimeoutSec 3
    Write-Output "Health Live Probe: $res"
}
catch {
    Write-Output "Probe result: $($_.Exception.Message)"
}
finally {
    if ($process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }
}
