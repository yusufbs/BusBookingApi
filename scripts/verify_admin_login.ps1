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
    $loginAdmin = @{ userName = "admin"; password = "Admin@123" } | ConvertTo-Json
    $res = Invoke-RestMethod -Uri "http://localhost:5241/api/Auth/login" -Method Post -Body $loginAdmin -ContentType "application/json" -TimeoutSec 10
    Write-Output "Login Result: $($res.result)"
    Write-Output "Message: $($res.message)"
    Write-Output "User: $($res.data.userName) | Role: $($res.data.role)"
    Write-Output "Token: $($res.data.token.Substring(0, 40))..."
}
catch {
    Write-Output "Login probe error: $($_.Exception.Message)"
}
finally {
    if ($process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
        Wait-Process -Id $process.Id -Timeout 2 -ErrorAction SilentlyContinue
    }
    Remove-Item $outLog, $errLog -Force -ErrorAction SilentlyContinue
}
