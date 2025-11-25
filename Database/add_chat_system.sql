-- Create Chat System for Shipper-Customer Communication
-- Run this script to add chat functionality

USE ValiModern;
GO

-- Create Messages table for chat
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Messages]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Messages] (
        [id] INT IDENTITY(1,1) PRIMARY KEY,
        [order_id] INT NOT NULL,
        [sender_id] INT NOT NULL,
        [receiver_id] INT NOT NULL,
        [message] NVARCHAR(MAX) NOT NULL,
        [is_read] BIT DEFAULT 0,
        [created_at] DATETIME DEFAULT GETDATE(),
        
        CONSTRAINT FK_Messages_Order FOREIGN KEY (order_id) REFERENCES Orders(id) ON DELETE CASCADE,
        CONSTRAINT FK_Messages_Sender FOREIGN KEY (sender_id) REFERENCES Users(id),
        CONSTRAINT FK_Messages_Receiver FOREIGN KEY (receiver_id) REFERENCES Users(id)
    );
    
    -- Create indexes for better performance
    CREATE NONCLUSTERED INDEX IX_Messages_OrderId ON Messages(order_id);
    CREATE NONCLUSTERED INDEX IX_Messages_CreatedAt ON Messages(created_at DESC);
    CREATE NONCLUSTERED INDEX IX_Messages_Unread ON Messages(receiver_id, is_read) WHERE is_read = 0;
    
    PRINT 'Created Messages table with indexes';
END
GO

-- Add unread message count column to Orders (denormalized for performance)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = 'shipper_unread_messages')
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD shipper_unread_messages INT DEFAULT 0;
    PRINT 'Added shipper_unread_messages column';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Orders]') AND name = 'customer_unread_messages')
BEGIN
    ALTER TABLE [dbo].[Orders]
    ADD customer_unread_messages INT DEFAULT 0;
    PRINT 'Added customer_unread_messages column';
END
GO

PRINT 'Chat system migration completed successfully!';
GO
