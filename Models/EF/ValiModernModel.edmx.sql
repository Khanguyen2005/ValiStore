
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 05/23/2026 19:16:45
-- Generated from EDMX file: C:\Users\Admin\Desktop\MyStudy\AspNet\ValiStoreSystem\Models\EF\ValiModernModel.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [ValiStoreDB];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_Colors_Products]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Colors] DROP CONSTRAINT [FK_Colors_Products];
GO
IF OBJECT_ID(N'[dbo].[FK_Messages_Order]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Messages] DROP CONSTRAINT [FK_Messages_Order];
GO
IF OBJECT_ID(N'[dbo].[FK_Messages_Receiver]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Messages] DROP CONSTRAINT [FK_Messages_Receiver];
GO
IF OBJECT_ID(N'[dbo].[FK_Messages_Sender]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Messages] DROP CONSTRAINT [FK_Messages_Sender];
GO
IF OBJECT_ID(N'[dbo].[FK_OD_Colors]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Order_Details] DROP CONSTRAINT [FK_OD_Colors];
GO
IF OBJECT_ID(N'[dbo].[FK_OD_Orders]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Order_Details] DROP CONSTRAINT [FK_OD_Orders];
GO
IF OBJECT_ID(N'[dbo].[FK_OD_Products]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Order_Details] DROP CONSTRAINT [FK_OD_Products];
GO
IF OBJECT_ID(N'[dbo].[FK_OD_Sizes]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Order_Details] DROP CONSTRAINT [FK_OD_Sizes];
GO
IF OBJECT_ID(N'[dbo].[FK_Orders_Shipper]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [FK_Orders_Shipper];
GO
IF OBJECT_ID(N'[dbo].[FK_Orders_Users]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Orders] DROP CONSTRAINT [FK_Orders_Users];
GO
IF OBJECT_ID(N'[dbo].[FK_Payments_Orders]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Payments] DROP CONSTRAINT [FK_Payments_Orders];
GO
IF OBJECT_ID(N'[dbo].[FK_PImages_Products]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Product_Images] DROP CONSTRAINT [FK_PImages_Products];
GO
IF OBJECT_ID(N'[dbo].[FK_Products_Brands]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Products] DROP CONSTRAINT [FK_Products_Brands];
GO
IF OBJECT_ID(N'[dbo].[FK_Products_Categories]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Products] DROP CONSTRAINT [FK_Products_Categories];
GO
IF OBJECT_ID(N'[dbo].[FK_Sizes_Products]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Sizes] DROP CONSTRAINT [FK_Sizes_Products];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Banners]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Banners];
GO
IF OBJECT_ID(N'[dbo].[Brands]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Brands];
GO
IF OBJECT_ID(N'[dbo].[Categories]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Categories];
GO
IF OBJECT_ID(N'[dbo].[Colors]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Colors];
GO
IF OBJECT_ID(N'[dbo].[Messages]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Messages];
GO
IF OBJECT_ID(N'[dbo].[Order_Details]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Order_Details];
GO
IF OBJECT_ID(N'[dbo].[Orders]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Orders];
GO
IF OBJECT_ID(N'[dbo].[Payments]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Payments];
GO
IF OBJECT_ID(N'[dbo].[Product_Images]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Product_Images];
GO
IF OBJECT_ID(N'[dbo].[Products]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Products];
GO
IF OBJECT_ID(N'[dbo].[Sizes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Sizes];
GO
IF OBJECT_ID(N'[dbo].[Users]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Users];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Banners'
CREATE TABLE [dbo].[Banners] (
    [id] int IDENTITY(1,1) NOT NULL,
    [name] nvarchar(200)  NOT NULL,
    [image_url] nvarchar(255)  NOT NULL
);
GO

-- Creating table 'Brands'
CREATE TABLE [dbo].[Brands] (
    [id] int IDENTITY(1,1) NOT NULL,
    [name] nvarchar(150)  NOT NULL,
    [image_url] nvarchar(255)  NULL
);
GO

-- Creating table 'Categories'
CREATE TABLE [dbo].[Categories] (
    [id] int IDENTITY(1,1) NOT NULL,
    [name] nvarchar(150)  NOT NULL
);
GO

-- Creating table 'Colors'
CREATE TABLE [dbo].[Colors] (
    [id] int IDENTITY(1,1) NOT NULL,
    [product_id] int  NOT NULL,
    [name] nvarchar(50)  NOT NULL,
    [color_code] nvarchar(20)  NULL
);
GO

-- Creating table 'Order_Details'
CREATE TABLE [dbo].[Order_Details] (
    [id] int IDENTITY(1,1) NOT NULL,
    [order_id] int  NOT NULL,
    [product_id] int  NOT NULL,
    [quantity] int  NOT NULL,
    [price] int  NOT NULL,
    [color_id] int  NULL,
    [size_id] int  NULL,
    [created_at] datetime  NOT NULL
);
GO

-- Creating table 'Orders'
CREATE TABLE [dbo].[Orders] (
    [id] int IDENTITY(1,1) NOT NULL,
    [user_id] int  NOT NULL,
    [order_date] datetime  NOT NULL,
    [status] nvarchar(20)  NOT NULL,
    [total_amount] bigint  NOT NULL,
    [phone] nvarchar(20)  NULL,
    [shipping_address] nvarchar(500)  NULL,
    [created_at] datetime  NOT NULL,
    [updated_at] datetime  NOT NULL,
    [shipper_id] int  NULL,
    [assigned_at] datetime  NULL,
    [delivered_at] datetime  NULL,
    [delivery_note] nvarchar(500)  NULL,
    [shipper_unread_messages] int  NULL,
    [customer_unread_messages] int  NULL
);
GO

-- Creating table 'Payments'
CREATE TABLE [dbo].[Payments] (
    [id] int IDENTITY(1,1) NOT NULL,
    [order_id] int  NOT NULL,
    [amount] bigint  NOT NULL,
    [payment_method] nvarchar(20)  NOT NULL,
    [status] nvarchar(20)  NOT NULL,
    [transaction_id] nvarchar(200)  NULL,
    [payment_date] datetime  NOT NULL
);
GO

-- Creating table 'Product_Images'
CREATE TABLE [dbo].[Product_Images] (
    [id] int IDENTITY(1,1) NOT NULL,
    [product_id] int  NOT NULL,
    [image_url] nvarchar(255)  NOT NULL,
    [is_main] bit  NOT NULL,
    [sort_order] int  NOT NULL
);
GO

-- Creating table 'Products'
CREATE TABLE [dbo].[Products] (
    [id] int IDENTITY(1,1) NOT NULL,
    [name] nvarchar(255)  NOT NULL,
    [description] nvarchar(max)  NULL,
    [image_url] nvarchar(255)  NULL,
    [original_price] int  NOT NULL,
    [price] int  NOT NULL,
    [stock] int  NOT NULL,
    [sold] int  NOT NULL,
    [category_id] int  NOT NULL,
    [brandId] int  NOT NULL,
    [is_active] bit  NOT NULL
);
GO

-- Creating table 'Sizes'
CREATE TABLE [dbo].[Sizes] (
    [id] int IDENTITY(1,1) NOT NULL,
    [product_id] int  NOT NULL,
    [name] nvarchar(20)  NOT NULL
);
GO

-- Creating table 'Users'
CREATE TABLE [dbo].[Users] (
    [id] int IDENTITY(1,1) NOT NULL,
    [username] nvarchar(100)  NULL,
    [email] nvarchar(255)  NOT NULL,
    [phone] nvarchar(20)  NULL,
    [is_admin] bit  NOT NULL,
    [address] nvarchar(500)  NULL,
    [created_at] datetime  NOT NULL,
    [updated_at] datetime  NOT NULL,
    [password] nvarchar(255)  NOT NULL,
    [role] nvarchar(20)  NULL
);
GO

-- Creating table 'Messages'
CREATE TABLE [dbo].[Messages] (
    [id] int IDENTITY(1,1) NOT NULL,
    [order_id] int  NOT NULL,
    [sender_id] int  NOT NULL,
    [receiver_id] int  NOT NULL,
    [message1] nvarchar(max)  NOT NULL,
    [is_read] bit  NULL,
    [created_at] datetime  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [id] in table 'Banners'
ALTER TABLE [dbo].[Banners]
ADD CONSTRAINT [PK_Banners]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'Brands'
ALTER TABLE [dbo].[Brands]
ADD CONSTRAINT [PK_Brands]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'Categories'
ALTER TABLE [dbo].[Categories]
ADD CONSTRAINT [PK_Categories]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'Colors'
ALTER TABLE [dbo].[Colors]
ADD CONSTRAINT [PK_Colors]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'Order_Details'
ALTER TABLE [dbo].[Order_Details]
ADD CONSTRAINT [PK_Order_Details]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'Orders'
ALTER TABLE [dbo].[Orders]
ADD CONSTRAINT [PK_Orders]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'Payments'
ALTER TABLE [dbo].[Payments]
ADD CONSTRAINT [PK_Payments]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'Product_Images'
ALTER TABLE [dbo].[Product_Images]
ADD CONSTRAINT [PK_Product_Images]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'Products'
ALTER TABLE [dbo].[Products]
ADD CONSTRAINT [PK_Products]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'Sizes'
ALTER TABLE [dbo].[Sizes]
ADD CONSTRAINT [PK_Sizes]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'Users'
ALTER TABLE [dbo].[Users]
ADD CONSTRAINT [PK_Users]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'Messages'
ALTER TABLE [dbo].[Messages]
ADD CONSTRAINT [PK_Messages]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [brandId] in table 'Products'
ALTER TABLE [dbo].[Products]
ADD CONSTRAINT [FK_Products_Brands]
    FOREIGN KEY ([brandId])
    REFERENCES [dbo].[Brands]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Products_Brands'
CREATE INDEX [IX_FK_Products_Brands]
ON [dbo].[Products]
    ([brandId]);
GO

-- Creating foreign key on [category_id] in table 'Products'
ALTER TABLE [dbo].[Products]
ADD CONSTRAINT [FK_Products_Categories]
    FOREIGN KEY ([category_id])
    REFERENCES [dbo].[Categories]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Products_Categories'
CREATE INDEX [IX_FK_Products_Categories]
ON [dbo].[Products]
    ([category_id]);
GO

-- Creating foreign key on [product_id] in table 'Colors'
ALTER TABLE [dbo].[Colors]
ADD CONSTRAINT [FK_Colors_Products]
    FOREIGN KEY ([product_id])
    REFERENCES [dbo].[Products]
        ([id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Colors_Products'
CREATE INDEX [IX_FK_Colors_Products]
ON [dbo].[Colors]
    ([product_id]);
GO

-- Creating foreign key on [color_id] in table 'Order_Details'
ALTER TABLE [dbo].[Order_Details]
ADD CONSTRAINT [FK_OD_Colors]
    FOREIGN KEY ([color_id])
    REFERENCES [dbo].[Colors]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_OD_Colors'
CREATE INDEX [IX_FK_OD_Colors]
ON [dbo].[Order_Details]
    ([color_id]);
GO

-- Creating foreign key on [order_id] in table 'Order_Details'
ALTER TABLE [dbo].[Order_Details]
ADD CONSTRAINT [FK_OD_Orders]
    FOREIGN KEY ([order_id])
    REFERENCES [dbo].[Orders]
        ([id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_OD_Orders'
CREATE INDEX [IX_FK_OD_Orders]
ON [dbo].[Order_Details]
    ([order_id]);
GO

-- Creating foreign key on [product_id] in table 'Order_Details'
ALTER TABLE [dbo].[Order_Details]
ADD CONSTRAINT [FK_OD_Products]
    FOREIGN KEY ([product_id])
    REFERENCES [dbo].[Products]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_OD_Products'
CREATE INDEX [IX_FK_OD_Products]
ON [dbo].[Order_Details]
    ([product_id]);
GO

-- Creating foreign key on [size_id] in table 'Order_Details'
ALTER TABLE [dbo].[Order_Details]
ADD CONSTRAINT [FK_OD_Sizes]
    FOREIGN KEY ([size_id])
    REFERENCES [dbo].[Sizes]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_OD_Sizes'
CREATE INDEX [IX_FK_OD_Sizes]
ON [dbo].[Order_Details]
    ([size_id]);
GO

-- Creating foreign key on [user_id] in table 'Orders'
ALTER TABLE [dbo].[Orders]
ADD CONSTRAINT [FK_Orders_Users]
    FOREIGN KEY ([user_id])
    REFERENCES [dbo].[Users]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Orders_Users'
CREATE INDEX [IX_FK_Orders_Users]
ON [dbo].[Orders]
    ([user_id]);
GO

-- Creating foreign key on [order_id] in table 'Payments'
ALTER TABLE [dbo].[Payments]
ADD CONSTRAINT [FK_Payments_Orders]
    FOREIGN KEY ([order_id])
    REFERENCES [dbo].[Orders]
        ([id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Payments_Orders'
CREATE INDEX [IX_FK_Payments_Orders]
ON [dbo].[Payments]
    ([order_id]);
GO

-- Creating foreign key on [product_id] in table 'Product_Images'
ALTER TABLE [dbo].[Product_Images]
ADD CONSTRAINT [FK_PImages_Products]
    FOREIGN KEY ([product_id])
    REFERENCES [dbo].[Products]
        ([id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_PImages_Products'
CREATE INDEX [IX_FK_PImages_Products]
ON [dbo].[Product_Images]
    ([product_id]);
GO

-- Creating foreign key on [product_id] in table 'Sizes'
ALTER TABLE [dbo].[Sizes]
ADD CONSTRAINT [FK_Sizes_Products]
    FOREIGN KEY ([product_id])
    REFERENCES [dbo].[Products]
        ([id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Sizes_Products'
CREATE INDEX [IX_FK_Sizes_Products]
ON [dbo].[Sizes]
    ([product_id]);
GO

-- Creating foreign key on [shipper_id] in table 'Orders'
ALTER TABLE [dbo].[Orders]
ADD CONSTRAINT [FK_Orders_Shipper]
    FOREIGN KEY ([shipper_id])
    REFERENCES [dbo].[Users]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Orders_Shipper'
CREATE INDEX [IX_FK_Orders_Shipper]
ON [dbo].[Orders]
    ([shipper_id]);
GO

-- Creating foreign key on [order_id] in table 'Messages'
ALTER TABLE [dbo].[Messages]
ADD CONSTRAINT [FK_Messages_Order]
    FOREIGN KEY ([order_id])
    REFERENCES [dbo].[Orders]
        ([id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Messages_Order'
CREATE INDEX [IX_FK_Messages_Order]
ON [dbo].[Messages]
    ([order_id]);
GO

-- Creating foreign key on [receiver_id] in table 'Messages'
ALTER TABLE [dbo].[Messages]
ADD CONSTRAINT [FK_Messages_Receiver]
    FOREIGN KEY ([receiver_id])
    REFERENCES [dbo].[Users]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Messages_Receiver'
CREATE INDEX [IX_FK_Messages_Receiver]
ON [dbo].[Messages]
    ([receiver_id]);
GO

-- Creating foreign key on [sender_id] in table 'Messages'
ALTER TABLE [dbo].[Messages]
ADD CONSTRAINT [FK_Messages_Sender]
    FOREIGN KEY ([sender_id])
    REFERENCES [dbo].[Users]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_Messages_Sender'
CREATE INDEX [IX_FK_Messages_Sender]
ON [dbo].[Messages]
    ([sender_id]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------