$process = Start-Process -FilePath "dotnet" -ArgumentList "C:\Projects\BusBookingApi\src\BusBooking.Api\bin\Debug\net10.0\BusBooking.Api.dll --urls http://localhost:5241" -PassThru
Start-Sleep -Seconds 5

try {
    $loginAdmin = @{ userName = "admin"; password = "Admin@123" } | ConvertTo-Json
    $res = Invoke-RestMethod -Uri "http://localhost:5241/api/BusBooking/login" -Method Post -Body $loginAdmin -ContentType "application/json" -TimeoutSec 10
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
        Stop-Process -Id $process.Id -Force
    }
}
