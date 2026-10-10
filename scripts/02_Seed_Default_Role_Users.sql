USE BusBookingDb;

-- 1. Ensure Admin User
IF EXISTS (SELECT 1 FROM dbo.Users WHERE UserName = 'admin')
BEGIN
    UPDATE dbo.Users 
    SET PasswordHash = '', PasswordSalt = 'SALT_ADMIN', Role = 'Admin' 
    WHERE UserName = 'admin';
END
ELSE
BEGIN
    INSERT INTO dbo.Users (UserName, EmailId, FullName, Role, PasswordHash, PasswordSalt, CreatedDate)
    VALUES ('admin', 'admin@busbooking.local', 'System Administrator', 'Admin', '', 'SALT_ADMIN', SYSUTCDATETIME());
END

-- 2. Ensure Manager User
IF EXISTS (SELECT 1 FROM dbo.Users WHERE UserName = 'manager')
BEGIN
    UPDATE dbo.Users 
    SET PasswordHash = '', PasswordSalt = 'SALT_MGR', Role = 'Manager' 
    WHERE UserName = 'manager';
END
ELSE
BEGIN
    INSERT INTO dbo.Users (UserName, EmailId, FullName, Role, PasswordHash, PasswordSalt, CreatedDate)
    VALUES ('manager', 'manager@busbooking.local', 'Operations Manager', 'Manager', '', 'SALT_MGR', SYSUTCDATETIME());
END

-- 3. Ensure Associate User
IF EXISTS (SELECT 1 FROM dbo.Users WHERE UserName = 'associate')
BEGIN
    UPDATE dbo.Users 
    SET PasswordHash = '', PasswordSalt = 'SALT_ASSOC', Role = 'Associate' 
    WHERE UserName = 'associate';
END
ELSE
BEGIN
    INSERT INTO dbo.Users (UserName, EmailId, FullName, Role, PasswordHash, PasswordSalt, CreatedDate)
    VALUES ('associate', 'associate@busbooking.local', 'Booking Associate', 'Associate', '', 'SALT_ASSOC', SYSUTCDATETIME());
END
