$process = Start-Process -FilePath "dotnet" -ArgumentList "C:\Projects\BusBookingApi\src\BusBooking.Api\bin\Debug\net10.0\BusBooking.Api.dll --urls http://localhost:5241" -PassThru
Start-Sleep -Seconds 3

try {
    # 1. Verify Swagger UI
    $swaggerUi = Invoke-WebRequest -Uri "http://localhost:5241/swagger/index.html" -TimeoutSec 5
    Write-Output "Swagger UI Status: $($swaggerUi.StatusCode)"

    # 2. Register Admin, Manager, Associate users
    $roles = @(
        @{ username = "admin_user"; email = "admin@busbooking.local"; role = "Admin"; pass = "Admin@123" },
        @{ username = "manager_user"; email = "manager@busbooking.local"; role = "Manager"; pass = "Manager@123" },
        @{ username = "associate_user"; email = "associate@busbooking.local"; role = "Associate"; pass = "Associate@123" }
    )

    foreach ($r in $roles) {
        $body = @{
            userId = 0
            userName = $r.username
            emailId = $r.email
            fullName = "$($r.role) User"
            role = $r.role
            password = $r.pass
        } | ConvertTo-Json

        try {
            $regResp = Invoke-RestMethod -Uri "http://localhost:5241/api/BusBooking/AddNewUser" -Method Post -Body $body -ContentType "application/json"
            Write-Output "Registered: $($r.username) -> Role: $($r.role) (Result: $($regResp.result))"
        } catch {
            Write-Output "Registration note: $($_.Exception.Message)"
        }
    }

    # 3. Login as Admin
    $loginAdmin = @{ userName = "admin_user"; password = "Admin@123" } | ConvertTo-Json
    $adminResp = Invoke-RestMethod -Uri "http://localhost:5241/api/BusBooking/login" -Method Post -Body $loginAdmin -ContentType "application/json"
    $adminToken = $adminResp.data.token
    Write-Output "Admin Login: Success (Token generated, length: $($adminToken.Length))"

    # 4. Access Admin-only endpoint with Admin token (GetAllUsers)
    $headersAdmin = @{ Authorization = "Bearer $adminToken" }
    $usersResp = Invoke-RestMethod -Uri "http://localhost:5241/api/BusBooking/GetAllUsers" -Headers $headersAdmin
    Write-Output "Admin GetAllUsers Authorized: Total users retrieved = $($usersResp.data.Count)"

    # 5. Login as Associate
    $loginAssociate = @{ userName = "associate_user"; password = "Associate@123" } | ConvertTo-Json
    $assocResp = Invoke-RestMethod -Uri "http://localhost:5241/api/BusBooking/login" -Method Post -Body $loginAssociate -ContentType "application/json"
    $assocToken = $assocResp.data.token
    Write-Output "Associate Login: Success (Role: $($assocResp.data.role))"

    # 6. Associate attempts to delete vendor (Admin only) -> Expect 403 Forbidden
    $headersAssoc = @{ Authorization = "Bearer $assocToken" }
    try {
        $delResp = Invoke-WebRequest -Uri "http://localhost:5241/api/BusBooking/DeleteBusVendor?id=999" -Method Delete -Headers $headersAssoc
        Write-Output "Delete status: $($delResp.StatusCode)"
    } catch {
        Write-Output "Associate deleting vendor forbidden as expected: $($_.Exception.Response.StatusCode)"
    }

    # 7. Associate accesses GetBusVendors (Allowed for Admin, Manager, Associate)
    $vendorsResp = Invoke-RestMethod -Uri "http://localhost:5241/api/BusBooking/GetBusVendors" -Headers $headersAssoc
    Write-Output "Associate GetBusVendors Authorized: Success (Vendors count: $($vendorsResp.Count))"
}
catch {
    Write-Output "Auth test error: $($_.Exception.Message)"
}
finally {
    if ($process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force
    }
}
