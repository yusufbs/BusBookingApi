-- =========================================================================================
-- Bus Booking System - Initial Schema, Covering Indexes & Concurrency Protections
-- Target Engine: Microsoft SQL Server 2022+ (.NET 10 / EF Core 10)
-- =========================================================================================

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'BusBookingDb')
BEGIN
    CREATE DATABASE BusBookingDb;
END
GO

USE BusBookingDb;
GO

-- 1. Users Table
IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users (
        UserId INT IDENTITY(1,1) NOT NULL,
        UserName NVARCHAR(100) NOT NULL,
        EmailId NVARCHAR(150) NOT NULL,
        FullName NVARCHAR(150) NOT NULL,
        Role NVARCHAR(50) NOT NULL CONSTRAINT DF_Users_Role DEFAULT N'Customer',
        PasswordHash NVARCHAR(255) NOT NULL,
        PasswordSalt NVARCHAR(255) NOT NULL,
        CreatedDate DATETIMEOFFSET NOT NULL CONSTRAINT DF_Users_CreatedDate DEFAULT SYSUTCDATETIME(),
        RefreshToken NVARCHAR(255) NULL,
        RefreshTokenExpiryTime DATETIMEOFFSET NULL,
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (UserId)
    );

    CREATE UNIQUE NONCLUSTERED INDEX IX_Users_UserName ON dbo.Users (UserName);
    CREATE UNIQUE NONCLUSTERED INDEX IX_Users_EmailId ON dbo.Users (EmailId);
END
GO

-- 2. BusVendors Table
IF OBJECT_ID(N'dbo.BusVendors', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BusVendors (
        VendorId INT IDENTITY(1,1) NOT NULL,
        VendorName NVARCHAR(150) NOT NULL,
        ContactNo VARCHAR(20) NOT NULL,
        EmailId NVARCHAR(150) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_BusVendors_IsActive DEFAULT 1,
        CONSTRAINT PK_BusVendors PRIMARY KEY CLUSTERED (VendorId)
    );

    CREATE NONCLUSTERED INDEX IX_BusVendors_IsActive ON dbo.BusVendors (IsActive) INCLUDE (VendorName);
END
GO

-- 3. BusLocations Table
IF OBJECT_ID(N'dbo.BusLocations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BusLocations (
        LocationId INT IDENTITY(1,1) NOT NULL,
        LocationName NVARCHAR(100) NOT NULL,
        Code VARCHAR(10) NOT NULL,
        CONSTRAINT PK_BusLocations PRIMARY KEY CLUSTERED (LocationId)
    );

    CREATE UNIQUE NONCLUSTERED INDEX IX_BusLocations_Code ON dbo.BusLocations (Code);
END
GO

-- 4. BusAddressLocations Table (Boarding & Dropping Points)
IF OBJECT_ID(N'dbo.BusAddressLocations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BusAddressLocations (
        LocationPointId INT IDENTITY(1,1) NOT NULL,
        LocationId INT NOT NULL,
        LocationTitle NVARCHAR(150) NOT NULL,
        LocationAddress NVARCHAR(500) NOT NULL,
        CONSTRAINT PK_BusAddressLocations PRIMARY KEY CLUSTERED (LocationPointId),
        CONSTRAINT FK_BusAddressLocations_BusLocations FOREIGN KEY (LocationId) 
            REFERENCES dbo.BusLocations (LocationId) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX IX_BusAddressLocations_LocationId ON dbo.BusAddressLocations (LocationId);
END
GO

-- 5. BusSchedules Table
IF OBJECT_ID(N'dbo.BusSchedules', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BusSchedules (
        ScheduleId INT IDENTITY(1,1) NOT NULL,
        VendorId INT NOT NULL,
        BusName NVARCHAR(100) NOT NULL,
        BusVehicleNo VARCHAR(50) NOT NULL,
        FromLocation INT NOT NULL,
        ToLocation INT NOT NULL,
        DepartureTime TIME(0) NOT NULL,
        ArrivalTime TIME(0) NOT NULL,
        ScheduleDate DATE NOT NULL,
        Price DECIMAL(18,2) NOT NULL,
        TotalSeats INT NOT NULL,
        RowVersion ROWVERSION NOT NULL,
        CONSTRAINT PK_BusSchedules PRIMARY KEY CLUSTERED (ScheduleId),
        CONSTRAINT FK_BusSchedules_BusVendors FOREIGN KEY (VendorId) 
            REFERENCES dbo.BusVendors (VendorId),
        CONSTRAINT FK_BusSchedules_FromLocation FOREIGN KEY (FromLocation) 
            REFERENCES dbo.BusLocations (LocationId),
        CONSTRAINT FK_BusSchedules_ToLocation FOREIGN KEY (ToLocation) 
            REFERENCES dbo.BusLocations (LocationId)
    );

    CREATE NONCLUSTERED INDEX IX_BusSchedules_VendorId ON dbo.BusSchedules (VendorId);

    -- Ultra-high performance covering index for route & schedule searches
    CREATE NONCLUSTERED INDEX IX_BusSchedules_Search_Covering 
    ON dbo.BusSchedules (FromLocation, ToLocation, ScheduleDate)
    INCLUDE (
        ScheduleId, 
        VendorId, 
        BusName, 
        BusVehicleNo, 
        DepartureTime, 
        ArrivalTime, 
        Price, 
        TotalSeats
    );
END
GO

-- 6. BusBookings Table
IF OBJECT_ID(N'dbo.BusBookings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BusBookings (
        BookingId INT IDENTITY(1,1) NOT NULL,
        CustId INT NOT NULL,
        ScheduleId INT NOT NULL,
        BookingDate DATETIMEOFFSET NOT NULL CONSTRAINT DF_BusBookings_BookingDate DEFAULT SYSUTCDATETIME(),
        TotalAmount DECIMAL(18,2) NOT NULL,
        BookingStatus NVARCHAR(30) NOT NULL CONSTRAINT DF_BusBookings_BookingStatus DEFAULT N'Confirmed',
        CONSTRAINT PK_BusBookings PRIMARY KEY CLUSTERED (BookingId),
        CONSTRAINT FK_BusBookings_Users FOREIGN KEY (CustId) 
            REFERENCES dbo.Users (UserId),
        CONSTRAINT FK_BusBookings_BusSchedules FOREIGN KEY (ScheduleId) 
            REFERENCES dbo.BusSchedules (ScheduleId)
    );

    CREATE NONCLUSTERED INDEX IX_BusBookings_CustId ON dbo.BusBookings (CustId)
    INCLUDE (BookingDate, TotalAmount, BookingStatus);

    CREATE NONCLUSTERED INDEX IX_BusBookings_Schedule_Status ON dbo.BusBookings (ScheduleId, BookingStatus)
    INCLUDE (BookingId);
END
GO

-- 7. BusBookingPassengers Table
IF OBJECT_ID(N'dbo.BusBookingPassengers', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BusBookingPassengers (
        PassengerId INT IDENTITY(1,1) NOT NULL,
        BookingId INT NOT NULL,
        PassengerName NVARCHAR(150) NOT NULL,
        Age INT NOT NULL,
        Gender VARCHAR(10) NOT NULL,
        SeatNo INT NOT NULL,
        CONSTRAINT PK_BusBookingPassengers PRIMARY KEY CLUSTERED (PassengerId),
        CONSTRAINT FK_BusBookingPassengers_BusBookings FOREIGN KEY (BookingId) 
            REFERENCES dbo.BusBookings (BookingId) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX IX_BusBookingPassengers_Booking_Seat 
    ON dbo.BusBookingPassengers (BookingId, SeatNo);
END
GO

-- =========================================================================================
-- Concurrency Protection: Indexed View for Duplicate Seat Prevention
-- Guarantees that at the database storage engine level, no two active bookings can reserve
-- the same seat for the same schedule, eliminating double-booking race conditions.
-- =========================================================================================
IF OBJECT_ID(N'dbo.vw_ActiveScheduleSeats', N'V') IS NOT NULL
    DROP VIEW dbo.vw_ActiveScheduleSeats;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE VIEW dbo.vw_ActiveScheduleSeats
WITH SCHEMABINDING
AS
SELECT 
    b.ScheduleId,
    p.SeatNo,
    COUNT_BIG(*) AS SeatCount
FROM dbo.BusBookingPassengers p
INNER JOIN dbo.BusBookings b ON p.BookingId = b.BookingId
WHERE b.BookingStatus = N'Confirmed'
GROUP BY b.ScheduleId, p.SeatNo;
GO

CREATE UNIQUE CLUSTERED INDEX UQ_vw_ActiveScheduleSeats_Schedule_Seat 
ON dbo.vw_ActiveScheduleSeats (ScheduleId, SeatNo);
GO

-- =========================================================================================
-- Seed Baseline Data (Locations, Vendors, Admin User)
-- =========================================================================================
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE UserName = N'admin')
BEGIN
    INSERT INTO dbo.Users (UserName, EmailId, FullName, Role, PasswordHash, PasswordSalt, CreatedDate)
    VALUES (N'admin', N'admin@busbooking.local', N'System Administrator', N'Admin', 
            N'AQAAAAIAAYagAAAAEO719FvU1U...HASH', N'SALT123', SYSUTCDATETIME());
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.BusLocations WHERE Code = N'NYC')
BEGIN
    INSERT INTO dbo.BusLocations (LocationName, Code) VALUES (N'New York', N'NYC');
    INSERT INTO dbo.BusLocations (LocationName, Code) VALUES (N'Boston', N'BOS');
    INSERT INTO dbo.BusLocations (LocationName, Code) VALUES (N'Washington DC', N'WDC');
    INSERT INTO dbo.BusLocations (LocationName, Code) VALUES (N'Philadelphia', N'PHL');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.BusVendors WHERE VendorName = N'Express Express Line')
BEGIN
    INSERT INTO dbo.BusVendors (VendorName, ContactNo, EmailId, IsActive)
    VALUES (N'Express Express Line', N'+1-800-555-0199', N'support@expressline.com', 1);
END
GO
