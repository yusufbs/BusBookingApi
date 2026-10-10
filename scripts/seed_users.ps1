Add-Type -Path "C:\Users\yusuf\.nuget\packages\bcrypt.net-next\4.2.0\lib\net48\BCrypt-Net-Next.dll"

$adminHash = [BCrypt.Net.BCrypt]::EnhancedHashPassword("Admin@123", 12)
$managerHash = [BCrypt.Net.BCrypt]::EnhancedHashPassword("Manager@123", 12)
$associateHash = [BCrypt.Net.BCrypt]::EnhancedHashPassword("Associate@123", 12)

Write-Output "Generated Admin Hash: $adminHash"

$sql = @"
USE BusBookingDb;

-- 1. Ensure Admin User
IF EXISTS (SELECT 1 FROM dbo.Users WHERE UserName = 'admin')
BEGIN
    UPDATE dbo.Users 
    SET PasswordHash = '$adminHash', PasswordSalt = 'SALT_ADMIN', Role = 'Admin' 
    WHERE UserName = 'admin';
END
ELSE
BEGIN
    INSERT INTO dbo.Users (UserName, EmailId, FullName, Role, PasswordHash, PasswordSalt, CreatedDate)
    VALUES ('admin', 'admin@busbooking.local', 'System Administrator', 'Admin', '$adminHash', 'SALT_ADMIN', SYSUTCDATETIME());
END

-- 2. Ensure Manager User
IF EXISTS (SELECT 1 FROM dbo.Users WHERE UserName = 'manager')
BEGIN
    UPDATE dbo.Users 
    SET PasswordHash = '$managerHash', PasswordSalt = 'SALT_MGR', Role = 'Manager' 
    WHERE UserName = 'manager';
END
ELSE
BEGIN
    INSERT INTO dbo.Users (UserName, EmailId, FullName, Role, PasswordHash, PasswordSalt, CreatedDate)
    VALUES ('manager', 'manager@busbooking.local', 'Operations Manager', 'Manager', '$managerHash', 'SALT_MGR', SYSUTCDATETIME());
END

-- 3. Ensure Associate User
IF EXISTS (SELECT 1 FROM dbo.Users WHERE UserName = 'associate')
BEGIN
    UPDATE dbo.Users 
    SET PasswordHash = '$associateHash', PasswordSalt = 'SALT_ASSOC', Role = 'Associate' 
    WHERE UserName = 'associate';
END
ELSE
BEGIN
    INSERT INTO dbo.Users (UserName, EmailId, FullName, Role, PasswordHash, PasswordSalt, CreatedDate)
    VALUES ('associate', 'associate@busbooking.local', 'Booking Associate', 'Associate', '$associateHash', 'SALT_ASSOC', SYSUTCDATETIME());
END
"@

Set-Content -Path "C:\Projects\BusBookingApi\scripts\02_Seed_Default_Role_Users.sql" -Value $sql -Encoding UTF8
sqlcmd -S localhost -E -C -i "C:\Projects\BusBookingApi\scripts\02_Seed_Default_Role_Users.sql"
Write-Output "Users successfully seeded with verified BCrypt passwords!"
