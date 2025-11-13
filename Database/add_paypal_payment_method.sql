-- Migration: Add PayPal to accepted payment methods
-- Date: 2025-01-13
-- Description: Update CHECK constraint on Payments table to include PayPal as a valid payment method

USE ValiModernDB;
GO

-- Step 1: Drop the existing CHECK constraint
ALTER TABLE Payments 
DROP CONSTRAINT CK_Payments_Method;
GO

-- Step 2: Add new CHECK constraint with PayPal included
ALTER TABLE Payments 
ADD CONSTRAINT CK_Payments_Method 
CHECK (payment_method IN (N'COD', N'VNPAY', N'PayPal'));
GO

-- Verify the change
PRINT 'Migration completed: PayPal payment method has been added to the database.';
PRINT 'Accepted payment methods: COD, VNPAY, PayPal';
GO
