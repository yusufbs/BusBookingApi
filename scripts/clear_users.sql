USE BusBookingDb;
DELETE FROM dbo.Users WHERE UserName IN ('admin', 'manager', 'associate');
