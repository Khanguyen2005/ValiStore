-- =============================================
-- Script: Reset Product Sold Count from Actual Completed Orders
-- Description: 
--   1. Reset ALL product.sold to 0
--   2. Recalculate sold count ONLY from Completed orders
--   3. Ensure data consistency across the system
-- =============================================

USE ValiModernDB;
GO

BEGIN TRANSACTION;

BEGIN TRY
    -- Step 1: Reset ALL product sold count to 0
    PRINT 'Step 1: Resetting all product sold counts to 0...';
    
    UPDATE Products
    SET sold = 0;
    
    PRINT 'Reset complete. All products now have sold = 0';
    
    -- Step 2: Recalculate sold count from Completed orders ONLY
    PRINT 'Step 2: Recalculating sold count from Completed orders...';
    
    UPDATE p
    SET p.sold = ISNULL(actual_sold.total_quantity, 0)
    FROM Products p
    LEFT JOIN (
        SELECT 
            od.product_id,
            SUM(od.quantity) as total_quantity
        FROM Order_Details od
        INNER JOIN Orders o ON od.order_id = o.id
        WHERE o.status = 'Completed'  -- ONLY Completed orders
        GROUP BY od.product_id
    ) actual_sold ON p.id = actual_sold.product_id;
    
    PRINT 'Recalculation complete.';
    
    -- Step 3: Show summary
    PRINT '============================================';
    PRINT 'Summary of Product Sold Counts:';
    PRINT '============================================';
    
    SELECT 
        COUNT(*) as total_products,
        SUM(CASE WHEN sold = 0 THEN 1 ELSE 0 END) as products_with_zero_sold,
        SUM(CASE WHEN sold > 0 THEN 1 ELSE 0 END) as products_with_actual_sold,
        SUM(sold) as total_sold_count
    FROM Products;
    
    PRINT '';
    PRINT 'Top 10 Products by Sold Count (from Completed orders):';
    SELECT TOP 10
        id,
        name,
        sold as sold_count,
        stock,
        price,
        is_active
    FROM Products
    ORDER BY sold DESC;
    
    -- Step 4: Verify consistency
    PRINT '';
    PRINT '============================================';
    PRINT 'Verifying Data Consistency:';
    PRINT '============================================';
    
    -- Check if any product has negative sold count (should be 0)
    IF EXISTS (SELECT 1 FROM Products WHERE sold < 0)
    BEGIN
        PRINT 'ERROR: Found products with negative sold count!';
        SELECT id, name, sold FROM Products WHERE sold < 0;
        ROLLBACK TRANSACTION;
        RETURN;
    END
    ELSE
    BEGIN
        PRINT 'OK: No products with negative sold count.';
    END
    
    -- Show products that might have fake data (sold > 0 but no completed orders)
    PRINT '';
    PRINT 'Products with sold > 0 (all from Completed orders):';
    SELECT 
        p.id,
        p.name,
        p.sold as current_sold,
        COUNT(DISTINCT od.order_id) as completed_order_count,
        SUM(od.quantity) as total_quantity_sold
    FROM Products p
    LEFT JOIN Order_Details od ON p.id = od.product_id
    LEFT JOIN Orders o ON od.order_id = o.id AND o.status = 'Completed'
    WHERE p.sold > 0
    GROUP BY p.id, p.name, p.sold
    ORDER BY p.sold DESC;
    
    COMMIT TRANSACTION;
    
    PRINT '';
    PRINT '============================================';
    PRINT 'SUCCESS: Product sold counts have been reset and recalculated!';
    PRINT '============================================';
    PRINT 'All sold counts are now based ONLY on Completed orders.';
    PRINT 'No fake data remaining.';
    
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    
    PRINT 'ERROR: Transaction rolled back!';
    PRINT 'Error Message: ' + ERROR_MESSAGE();
    PRINT 'Error Line: ' + CAST(ERROR_LINE() AS NVARCHAR(10));
END CATCH;

GO

-- =============================================
-- Verification Query (run separately to verify)
-- =============================================
/*
-- Verify that sold count matches actual completed orders
SELECT 
    p.id,
    p.name,
    p.sold as current_sold_in_db,
    ISNULL(SUM(od.quantity), 0) as actual_sold_from_completed_orders,
    CASE 
        WHEN p.sold = ISNULL(SUM(od.quantity), 0) THEN 'MATCH ?'
        ELSE 'MISMATCH ?'
    END as status
FROM Products p
LEFT JOIN Order_Details od ON p.id = od.product_id
LEFT JOIN Orders o ON od.order_id = o.id AND o.status = 'Completed'
GROUP BY p.id, p.name, p.sold
HAVING p.sold != ISNULL(SUM(od.quantity), 0)  -- Show only mismatches
ORDER BY p.sold DESC;

-- If no rows returned, all data is consistent! ?
*/
