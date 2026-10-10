$process = Start-Process -FilePath "dotnet" -ArgumentList "C:\Projects\BusBookingApi\src\BusBooking.Api\bin\Debug\net10.0\BusBooking.Api.dll --urls http://localhost:5241" -PassThru
Start-Sleep -Seconds 3

try {
    $swaggerJson = Invoke-RestMethod -Uri "http://localhost:5241/swagger/v1/swagger.json" -TimeoutSec 5
    Write-Output "Swagger Title: $($swaggerJson.info.title)"
    Write-Output "Swagger Version: $($swaggerJson.info.version)"
    Write-Output "Total Documented Paths in Swagger: $($swaggerJson.paths.PSObject.Properties.Name.Count)"

    $swaggerUi = Invoke-WebRequest -Uri "http://localhost:5241/swagger/index.html" -TimeoutSec 5
    Write-Output "Swagger UI Status: $($swaggerUi.StatusCode)"
}
catch {
    Write-Output "Swagger probe failed: $($_.Exception.Message)"
}
finally {
    if ($process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }
}
