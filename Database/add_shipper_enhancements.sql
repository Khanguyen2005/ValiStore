-- Add columns for Shipper enhancements
-- Run this script to add new features to Shipper module

USE ValiModern;
GO

-- Add delivery photos column to Orders table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = 'delivery_photos')
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD delivery_photos NVARCHAR(MAX) NULL; -- JSON array of photo URLs
    PRINT 'Added delivery_photos column';
END
GO

-- Add coordinates columns to Orders table for mapping
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = 'latitude')
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD latitude DECIMAL(10, 8) NULL;
    PRINT 'Added latitude column';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = 'longitude')
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD longitude DECIMAL(11, 8) NULL;
    PRINT 'Added longitude column';
END
GO

-- Create index for location-based queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Orders_Location' AND object_id = OBJECT_ID('Orders'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Orders_Location
    ON [dbo].[Orders] (latitude, longitude)
    WHERE latitude IS NOT NULL AND longitude IS NOT NULL;
    PRINT 'Created IX_Orders_Location index';
END
GO

-- Add customer phone to Orders if not exists (for quick call)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = 'customer_phone')
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD customer_phone NVARCHAR(20) NULL;
    PRINT 'Added customer_phone column';
END
GO

PRINT 'Shipper enhancement migration completed successfully!';
GO
