-- =============================================
-- Stored Procedure: Recalculate Product Sold Count
-- Description: 
--   Automatically recalculate sold count for products
--   based ONLY on Completed orders
-- =============================================

USE ValiModernDB;
GO

-- Drop existing procedure if exists
IF OBJECT_ID('sp_RecalculateProductSold', 'P') IS NOT NULL
    DROP PROCEDURE sp_RecalculateProductSold;
GO

CREATE PROCEDURE sp_RecalculateProductSold
    @ProductId INT = NULL  -- NULL = recalculate ALL products
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        IF @ProductId IS NULL
        BEGIN
            -- Recalculate ALL products
            PRINT 'Recalculating sold count for ALL products...';
            
            UPDATE p
            SET p.sold = ISNULL(actual_sold.total_quantity, 0)
            FROM Products p
            LEFT JOIN (
                SELECT 
                    od.product_id,
                    SUM(od.quantity) as total_quantity
                FROM Order_Details od
                INNER JOIN Orders o ON od.order_id = o.id
                WHERE o.status = 'Completed'
                GROUP BY od.product_id
            ) actual_sold ON p.id = actual_sold.product_id;
            
            PRINT 'Recalculation complete for all products.';
        END
        ELSE
        BEGIN
            -- Recalculate specific product
            PRINT 'Recalculating sold count for Product ID: ' + CAST(@ProductId AS VARCHAR(10));
            
            DECLARE @ActualSold INT;
            
            SELECT @ActualSold = ISNULL(SUM(od.quantity), 0)
            FROM Order_Details od
            INNER JOIN Orders o ON od.order_id = o.id
            WHERE od.product_id = @ProductId 
              AND o.status = 'Completed';
            
            UPDATE Products
            SET sold = @ActualSold
            WHERE id = @ProductId;
            
            PRINT 'Product ID ' + CAST(@ProductId AS VARCHAR(10)) + ' sold count updated to: ' + CAST(@ActualSold AS VARCHAR(10));
        END
        
        COMMIT TRANSACTION;
        
        -- Return summary
        IF @ProductId IS NULL
        BEGIN
            SELECT 
                'Summary' as info,
                COUNT(*) as total_products,
                SUM(sold) as total_sold_count,
                AVG(sold) as avg_sold_per_product
            FROM Products;
        END
        ELSE
        BEGIN
            SELECT 
                id,
                name,
                sold,
                stock,
                price
            FROM Products
            WHERE id = @ProductId;
        END
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorLine INT = ERROR_LINE();
        
        RAISERROR('Error in sp_RecalculateProductSold: %s (Line %d)', 16, 1, @ErrorMessage, @ErrorLine);
    END CATCH
END;
GO

PRINT 'Stored procedure sp_RecalculateProductSold created successfully!';
PRINT '';
PRINT 'Usage:';
PRINT '  -- Recalculate ALL products:';
PRINT '  EXEC sp_RecalculateProductSold;';
PRINT '';
PRINT '  -- Recalculate specific product (e.g., Product ID 5):';
PRINT '  EXEC sp_RecalculateProductSold @ProductId = 5;';
GO

-- Test the procedure
PRINT '';
PRINT '============================================';
PRINT 'Testing stored procedure...';
PRINT '============================================';

EXEC sp_RecalculateProductSold;

GO
