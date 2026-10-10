$process = Start-Process -FilePath "dotnet" -ArgumentList "C:\Projects\BusBookingApi\src\BusBooking.Api\bin\Debug\net10.0\BusBooking.Api.dll --urls http://localhost:5241" -PassThru
Start-Sleep -Seconds 3

try {
    $accounts = @(
        @{ username = "admin"; email = "admin@busbooking.local"; role = "Admin"; pass = "Admin@123"; name = "System Administrator" },
        @{ username = "manager"; email = "manager@busbooking.local"; role = "Manager"; pass = "Manager@123"; name = "Operations Manager" },
        @{ username = "associate"; email = "associate@busbooking.local"; role = "Associate"; pass = "Associate@123"; name = "Booking Associate" }
    )

    foreach ($acc in $accounts) {
        $body = @{
            userId = 0
            userName = $acc.username
            emailId = $acc.email
            fullName = $acc.name
            role = $acc.role
            password = $acc.pass
        } | ConvertTo-Json

        $regResp = Invoke-RestMethod -Uri "http://localhost:5241/api/BusBooking/AddNewUser" -Method Post -Body $body -ContentType "application/json"
        Write-Output "User created: $($acc.username) [Role: $($acc.role)] -> Result: $($regResp.result)"
    }

    # Verify Login for admin
    $loginAdmin = @{ userName = "admin"; password = "Admin@123" } | ConvertTo-Json
    $loginResp = Invoke-RestMethod -Uri "http://localhost:5241/api/BusBooking/login" -Method Post -Body $loginAdmin -ContentType "application/json"
    Write-Output "Admin Login Test: $($loginResp.message)"
    Write-Output "Admin Token Generated: $($loginResp.data.token.Substring(0, 30))..."
}
catch {
    Write-Output "Error: $($_.Exception.Message)"
}
finally {
    if ($process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }
}
