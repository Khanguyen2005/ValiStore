-- =============================================
-- Add Database Constraints for Product Sold Integrity
-- Ensures sold count is always >= 0 and managed correctly
-- =============================================

USE ValiModernDB;
GO

-- 1. Add CHECK constraint to ensure sold is never negative
IF NOT EXISTS (
    SELECT 1 
    FROM sys.check_constraints 
    WHERE name = 'CK_Product_Sold_NonNegative'
)
BEGIN
    PRINT 'Adding CHECK constraint: CK_Product_Sold_NonNegative';
    
    ALTER TABLE Products
    ADD CONSTRAINT CK_Product_Sold_NonNegative 
    CHECK (sold >= 0);
    
    PRINT 'Constraint added successfully.';
END
ELSE
BEGIN
    PRINT 'Constraint CK_Product_Sold_NonNegative already exists.';
END

GO

-- 2. Create index on Orders.status for better performance
IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'IX_Orders_Status' 
    AND object_id = OBJECT_ID('Orders')
)
BEGIN
    PRINT 'Creating index: IX_Orders_Status';
    
    CREATE NONCLUSTERED INDEX IX_Orders_Status
    ON Orders(status)
    INCLUDE (id, order_date, total_amount);
    
    PRINT 'Index created successfully.';
END
ELSE
BEGIN
    PRINT 'Index IX_Orders_Status already exists.';
END

GO

-- 3. Create index on Order_Details for sold calculation
IF NOT EXISTS (
    SELECT 1 
    FROM sys.indexes 
    WHERE name = 'IX_OrderDetails_ProductId_OrderId' 
    AND object_id = OBJECT_ID('Order_Details')
)
BEGIN
    PRINT 'Creating index: IX_OrderDetails_ProductId_OrderId';
    
    CREATE NONCLUSTERED INDEX IX_OrderDetails_ProductId_OrderId
    ON Order_Details(product_id, order_id)
    INCLUDE (quantity, price);
    
    PRINT 'Index created successfully.';
END
ELSE
BEGIN
    PRINT 'Index IX_OrderDetails_ProductId_OrderId already exists.';
END

GO

PRINT '';
PRINT '============================================';
PRINT 'Database constraints and indexes configured!';
PRINT '============================================';
PRINT 'Products.sold will always be >= 0';
PRINT 'Performance optimized for sold count queries';
