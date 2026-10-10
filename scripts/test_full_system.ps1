$process = Start-Process -FilePath "dotnet" -ArgumentList "C:\Projects\BusBookingApi\src\BusBooking.Api\bin\Debug\net10.0\BusBooking.Api.dll --urls http://localhost:5241" -PassThru
Start-Sleep -Seconds 3

try {
    $resLive = Invoke-RestMethod -Uri "http://localhost:5241/health/live" -TimeoutSec 3
    Write-Output "Health Live: $resLive"

    $resReady = Invoke-RestMethod -Uri "http://localhost:5241/health/ready" -TimeoutSec 5
    Write-Output "Health Ready (SQL Server): $resReady"

    $resLocations = Invoke-RestMethod -Uri "http://localhost:5241/api/BusBooking/GetBusLocations" -TimeoutSec 5
    Write-Output "GetBusLocations count: $($resLocations.Count)"
    Write-Output ($resLocations | ConvertTo-Json)
}
catch {
    Write-Output "Probe failed: $($_.Exception.Message)"
}
finally {
    if ($process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }
}
