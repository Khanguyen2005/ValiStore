-- =============================================
-- Migration: Change all money columns to INT
-- Date: 2024
-- Description: Convert DECIMAL(12,2) columns to INT for VND currency
-- WARNING: This will CLEAR existing data in affected tables to avoid conversion errors
-- =============================================

USE [ValiModernDB];  -- Changed from ValiModern to ValiModernDB
GO

PRINT '============================================';
PRINT 'Starting migration: DECIMAL -> INT for money columns';
PRINT 'WARNING: Clearing data to avoid conversion errors';
PRINT '============================================';
GO

-- Step 1: Clear data from tables (to avoid decimal conversion errors)
-- Order matters due to foreign keys
PRINT 'Clearing existing data...';

DELETE FROM Payments;
DELETE FROM Order_Details;
DELETE FROM Orders;
DELETE FROM Product_Images;
DELETE FROM Colors;
DELETE FROM Sizes;
DELETE FROM Products;

PRINT 'Data cleared successfully.';
GO

-- Step 2: Alter column types
PRINT 'Converting Products.price: DECIMAL -> INT';
ALTER TABLE Products ALTER COLUMN price INT NOT NULL;
GO

PRINT 'Converting Products.original_price: DECIMAL -> INT';
ALTER TABLE Products ALTER COLUMN original_price INT NOT NULL;
GO

PRINT 'Converting Orders.total_amount: DECIMAL -> BIGINT';
ALTER TABLE Orders ALTER COLUMN total_amount BIGINT NOT NULL;
GO

PRINT 'Converting Order_Details.price: DECIMAL -> INT';
ALTER TABLE Order_Details ALTER COLUMN price INT NOT NULL;
GO

PRINT 'Converting Payments.amount: DECIMAL -> BIGINT';
ALTER TABLE Payments ALTER COLUMN amount BIGINT NOT NULL;
GO

PRINT '============================================';
PRINT 'Migration completed successfully!';
PRINT 'All money columns converted to INT/BIGINT';
PRINT 'NOTE: Data was cleared. You can now insert new data.';
PRINT '============================================';
GO
