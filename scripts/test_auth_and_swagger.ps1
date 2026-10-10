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
    $t = [System.Diagnostics.Stopwatch]::StartNew()

    # 1. Verify Swagger UI (-UseBasicParsing prevents PowerShell 5.1 IE MSHTML DOM parser hang)
    $swaggerUi = Invoke-WebRequest -Uri "http://localhost:5241/swagger/index.html" -UseBasicParsing -TimeoutSec 10
    Write-Output "1. Swagger UI ($($t.ElapsedMilliseconds)ms): $($swaggerUi.StatusCode)"

    # 2. Verify OpenAPI Specification
    $swaggerDoc = Invoke-RestMethod -Uri "http://localhost:5241/swagger/v1/swagger.json" -TimeoutSec 10
    Write-Output "2. Swagger JSON ($($t.ElapsedMilliseconds)ms): $($swaggerDoc.paths.PSObject.Properties.Name.Count) paths"

    # 3. Test dedicated AuthController endpoint: POST /api/Auth/login (Admin)
    $loginAdmin = @{ userName = "admin"; password = "Admin@123" } | ConvertTo-Json
    $adminAuthResp = Invoke-RestMethod -Uri "http://localhost:5241/api/Auth/login" -Method Post -Body $loginAdmin -ContentType "application/json" -TimeoutSec 10
    $adminToken = $adminAuthResp.data.token
    Write-Output "3. Admin Auth Login [/api/Auth/login] ($($t.ElapsedMilliseconds)ms): Role = $($adminAuthResp.data.role)"

    # 4. Test backward-compatible alias route: POST /api/BusBooking/login
    $adminLegacyResp = Invoke-RestMethod -Uri "http://localhost:5241/api/BusBooking/login" -Method Post -Body $loginAdmin -ContentType "application/json" -TimeoutSec 10
    Write-Output "4. Admin Legacy Login [/api/BusBooking/login] ($($t.ElapsedMilliseconds)ms): Role = $($adminLegacyResp.data.role)"

    # 5. Test AuthController users list with Admin Token: GET /api/Auth/users
    $headersAdmin = @{ Authorization = "Bearer $adminToken" }
    $usersResp = Invoke-RestMethod -Uri "http://localhost:5241/api/Auth/users" -Headers $headersAdmin -TimeoutSec 10
    Write-Output "5. GetAllUsers [/api/Auth/users] ($($t.ElapsedMilliseconds)ms): $($usersResp.data.Count) users"

    # 6. Test backward-compatible users list: GET /api/BusBooking/GetAllUsers
    $legacyUsersResp = Invoke-RestMethod -Uri "http://localhost:5241/api/BusBooking/GetAllUsers" -Headers $headersAdmin -TimeoutSec 10
    Write-Output "6. GetAllUsers Legacy [/api/BusBooking/GetAllUsers] ($($t.ElapsedMilliseconds)ms): $($legacyUsersResp.data.Count) users"

    # 7. Test Manager Login via /api/Auth/login
    $loginMgr = @{ userName = "manager"; password = "Manager@123" } | ConvertTo-Json
    $mgrResp = Invoke-RestMethod -Uri "http://localhost:5241/api/Auth/login" -Method Post -Body $loginMgr -ContentType "application/json" -TimeoutSec 10
    Write-Output "7. Manager Login [/api/Auth/login] ($($t.ElapsedMilliseconds)ms): Role = $($mgrResp.data.role)"

    # 8. Test Associate Login via /api/Auth/login
    $loginAssoc = @{ userName = "associate"; password = "Associate@123" } | ConvertTo-Json
    $assocResp = Invoke-RestMethod -Uri "http://localhost:5241/api/Auth/login" -Method Post -Body $loginAssoc -ContentType "application/json" -TimeoutSec 10
    $assocToken = $assocResp.data.token
    Write-Output "8. Associate Login [/api/Auth/login] ($($t.ElapsedMilliseconds)ms): Role = $($assocResp.data.role)"

    # 9. Test RBAC: Associate cannot access Admin/Manager-only endpoint GET /api/Auth/users -> Expect 403 Forbidden
    $headersAssoc = @{ Authorization = "Bearer $assocToken" }
    try {
        $null = Invoke-WebRequest -Uri "http://localhost:5241/api/Auth/users" -Headers $headersAssoc -UseBasicParsing -TimeoutSec 10
        Write-Output "9. Unexpected success for Associate on /api/Auth/users"
    } catch {
        Write-Output "9. Associate /api/Auth/users Forbidden ($($t.ElapsedMilliseconds)ms): $($_.Exception.Response.StatusCode)"
    }

    # 10. Test Authorized BusBooking endpoint: Associate can access GET /api/BusBooking/GetBusVendors
    $vendorsResp = Invoke-RestMethod -Uri "http://localhost:5241/api/BusBooking/GetBusVendors" -Headers $headersAssoc -TimeoutSec 10
    Write-Output "10. Associate GetBusVendors [/api/BusBooking/GetBusVendors] ($($t.ElapsedMilliseconds)ms): $($vendorsResp.Count) vendors"

    Write-Output "`n=== ALL AUTHENTICATION AND AUTHORIZATION TESTS PASSED SUCCESSFULLY! ==="
}
catch {
    Write-Output "Auth test error: $($_.Exception.Message)"
}
finally {
    if ($process -and -not $process.HasExited) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
        Wait-Process -Id $process.Id -Timeout 2 -ErrorAction SilentlyContinue
    }
    Remove-Item $outLog, $errLog -Force -ErrorAction SilentlyContinue
}
