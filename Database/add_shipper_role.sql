-- =====================================================
-- Script: Thêm ch?c n?ng SHIPPER vào h? th?ng
-- Date: 2025-11-24
-- Description: Thêm c?t role vào Users, shipper_id vào Orders
-- =====================================================

USE [ValiModernDB]
GO

-- B??C 1: Thêm c?t 'role' vào b?ng Users
-- Giá tr?: 'customer', 'admin', 'shipper'
ALTER TABLE [dbo].[Users]
ADD [role] NVARCHAR(20) NULL;
GO

-- B??C 2: Migrate d? li?u hi?n t?i t? is_admin sang role
UPDATE [dbo].[Users]
SET [role] = CASE 
    WHEN [is_admin] = 1 THEN 'admin'
    ELSE 'customer'
END;
GO

-- B??C 3: Set default cho role
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [DF_Users_Role] DEFAULT ('customer') FOR [role];
GO

-- B??C 4: Thêm các c?t liên quan ??n shipper vào b?ng Orders
ALTER TABLE [dbo].[Orders]
ADD 
    [shipper_id] INT NULL,
    [assigned_at] DATETIME2(0) NULL,
    [delivered_at] DATETIME2(0) NULL,
    [delivery_note] NVARCHAR(500) NULL;
GO

-- B??C 5: T?o Foreign Key cho shipper_id
ALTER TABLE [dbo].[Orders]
ADD CONSTRAINT [FK_Orders_Shipper] 
FOREIGN KEY ([shipper_id]) REFERENCES [dbo].[Users]([id]);
GO

-- B??C 6: T?o Index cho shipper_id ?? t?ng hi?u su?t query
CREATE NONCLUSTERED INDEX [IX_Orders_Shipper] ON [dbo].[Orders]
(
    [shipper_id] ASC
) WHERE [shipper_id] IS NOT NULL;
GO

-- B??C 7: T?o 2 tài kho?n shipper m?u
INSERT INTO [dbo].[Users] ([username], [email], [phone], [password], [is_admin], [role], [address], [created_at], [updated_at])
VALUES 
    (N'Nguy?n V?n Giao', N'shipper1@valimodern.com', N'0901234567', N'123456', 0, 'shipper', N'TP. H? Chí Minh', GETDATE(), GETDATE()),
    (N'Tr?n Th? Hàng', N'shipper2@valimodern.com', N'0901234568', N'123456', 0, 'shipper', N'Hà N?i', GETDATE(), GETDATE());
GO

-- B??C 8: Thêm Check Constraint cho role
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [CK_Users_Role] CHECK ([role] IN ('customer', 'admin', 'shipper'));
GO

PRINT '?ã c?p nh?t database thành công!';
PRINT '- ?ã thêm c?t role vào Users';
PRINT '- ?ã thêm shipper_id, assigned_at, delivered_at, delivery_note vào Orders';
PRINT '- ?ã t?o 2 tài kho?n shipper:';
PRINT '  + shipper1@valimodern.com / 123456';
PRINT '  + shipper2@valimodern.com / 123456';
GO
