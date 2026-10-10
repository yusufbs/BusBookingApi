$apiDir = "C:\Projects\BusBookingApi\src\BusBooking.Api"
$outLog = "C:\Projects\BusBookingApi\scripts\api_stdout.log"
$errLog = "C:\Projects\BusBookingApi\scripts\api_stderr.log"

$process = Start-Process -FilePath "dotnet" `
    -ArgumentList "C:\Projects\BusBookingApi\src\BusBooking.Api\bin\Debug\net10.0\BusBooking.Api.dll --urls http://localhost:5241" `
    -RedirectStandardOutput $outLog `
    -RedirectStandardError $errLog `
    -WorkingDirectory $apiDir `
    -PassThru

Start-Sleep -Seconds 2

try {
    $swaggerJson = Invoke-RestMethod -Uri "http://localhost:5241/swagger/v1/swagger.json" -TimeoutSec 5
    Write-Output "Swagger Title: $($swaggerJson.info.title)"
    Write-Output "Swagger Version: $($swaggerJson.info.version)"
    Write-Output "Total Documented Paths in Swagger: $($swaggerJson.paths.PSObject.Properties.Name.Count)"

    $swaggerUi = Invoke-WebRequest -Uri "http://localhost:5241/swagger/index.html" -UseBasicParsing -TimeoutSec 5
    Write-Output "Swagger UI Status: $($swaggerUi.StatusCode)"
}
catch {
    Write-Output "Swagger probe failed: $($_.Exception.Message)"
}
finally {
    if ($process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
        Wait-Process -Id $process.Id -Timeout 2 -ErrorAction SilentlyContinue
    }
    Remove-Item $outLog, $errLog -Force -ErrorAction SilentlyContinue
}
