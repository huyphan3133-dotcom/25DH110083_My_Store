--New Database: MyStore
-- Bang User: quan ly ca tai khoan cua Khach hang va Admin
CREATE TABLE [dbo].[User] (
    [Username]     NVARCHAR (255) NOT NULL,
    [Password] NCHAR (50)     NOT NULL,
   [UserRole]     NCHAR (1) NOT NULL, -- A: admin, C: customer
    PRIMARY KEY CLUSTERED ([Username] ASC)
);

--Bang Customer: luu thong tin khach hang
CREATE TABLE [dbo].[Customer] (
    [CustomerID]          INT IDENTITY (1, 1) NOT NULL,
    [CustomerName]     NVARCHAR (MAX) NOT NULL,
    [CustomerPhone]    NVARCHAR (15)  NOT NULL,
    [CustomerEmail]      NVARCHAR (MAX) NULL,
    [CustomerAddress]  NVARCHAR (MAX) NULL,
    [Username]              NVARCHAR (255) NOT NULL,
    PRIMARY KEY CLUSTERED ([CustomerID] ASC),
    CONSTRAINT [FK_User_Customer] FOREIGN KEY ([Username]) REFERENCES [dbo].[User] ([Username])
);

--Bang Category: luu thong tin danh muc san pham
CREATE TABLE [dbo].[Category] (
    [CategoryID]            INT            IDENTITY (1, 1) NOT NULL,
    [CategoryName]      NVARCHAR (MAX) NOT NULL,
    PRIMARY KEY CLUSTERED ([CategoryID] ASC)
);
--Bang Product: luu thong tin san pham
CREATE TABLE [dbo].[Product] (
    [ProductID]              INT             IDENTITY (1, 1) NOT NULL,
    [CategoryID]            INT            NOT NULL,
    [ProductName]        NVARCHAR (MAX)  NOT NULL,
    [ProductDecription]  NVARCHAR (MAX)  NOT NULL,
    [ProductPrice]          DECIMAL (18, 2) NOT NULL,
    [ProductImage]        NVARCHAR (MAX)  NULL,
    PRIMARY KEY CLUSTERED ([ProductID] ASC),
    CONSTRAINT [FK_Pro_Category] FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[Category] ([CategoryID])
);

--Bang Order: luu thong tin don hang cua khach hang
CREATE TABLE [dbo].[Order] (
    [OrderID]                 INT            IDENTITY (1, 1) NOT NULL,
    [CustomerID]          INT            NOT NULL,
    [OrderDate]             DATE           NOT NULL,
    [TotalAmount]         DECIMAL (18, 2) NOT NULL,
    [PaymentStatus]     NVARCHAR (MAX) NULL,
    [AddressDelivery]   NVARCHAR (MAX) NOT NULL,
    PRIMARY KEY CLUSTERED ([OrderID] ASC),
    FOREIGN KEY ([CustomerID]) REFERENCES [dbo].[Customer] ([CustomerID])
);
--Bang OrderDetail: luu thong tin chi tiet cac san pham co trong don hang
CREATE TABLE [dbo].[OrderDetail] (
    [ID]        INT        IDENTITY (1, 1) NOT NULL,
    [ProductID] INT        NOT NULL,
    [OrderID]   INT        NOT NULL,
    [Quantity]  INT        NOT NULL,
    [UnitPrice] DECIMAL (18, 2) NOT NULL,
    PRIMARY KEY CLUSTERED ([ID] ASC),
    FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Product] ([ProductID]),
    FOREIGN KEY ([OrderID]) REFERENCES [dbo].[Order] ([OrderID])
);

ALTER TABLE [Order] ADD OrderStatus NVARCHAR(50) NULL;
UPDATE [Order] SET OrderStatus = N'Chờ xử lý';
ALTER AUTHORIZATION ON DATABASE::MyStore TO sa;
USE MyStore;
GO

-- Bảng Order: đổi AddressDelivery -> ShippingAddress, thêm DeliveryMethod, PaymentMethod
IF COL_LENGTH('dbo.[Order]', 'AddressDelivery') IS NOT NULL AND COL_LENGTH('dbo.[Order]', 'ShippingAddress') IS NULL
    EXEC sp_rename 'dbo.[Order].AddressDelivery', 'ShippingAddress', 'COLUMN';
GO
IF COL_LENGTH('dbo.[Order]', 'DeliveryMethod') IS NULL
    ALTER TABLE dbo.[Order] ADD DeliveryMethod NVARCHAR(MAX) NULL;
IF COL_LENGTH('dbo.[Order]', 'PaymentMethod') IS NULL
    ALTER TABLE dbo.[Order] ADD PaymentMethod NVARCHAR(MAX) NULL;
GO

-- Bảng OrderDetail: thêm TotalPrice tự tính
IF COL_LENGTH('dbo.OrderDetail', 'TotalPrice') IS NULL
    ALTER TABLE dbo.OrderDetail ADD TotalPrice AS (Quantity * UnitPrice) PERSISTED;
GO

-- Bảng Product: thêm số lượng tồn kho
IF COL_LENGTH('dbo.Product', 'StockQuantity') IS NULL
    ALTER TABLE dbo.Product ADD StockQuantity INT NOT NULL CONSTRAINT DF_Product_StockQuantity DEFAULT (100);
GO
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = 'CK_Product_StockQuantity')
    ALTER TABLE dbo.Product ADD CONSTRAINT CK_Product_StockQuantity CHECK (StockQuantity >= 0);
GO
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Product';
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Order';
USE MyStore; -- Ghi đè tên DB cho chắc chắn


SELECT COLUMN_NAME 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Order';


ALTER TABLE [Order] ADD OrderStatus NVARCHAR(50) NULL;
UPDATE [Order] SET OrderStatus = N'Chờ xử lý';


SELECT * FROM [Order];
ALTER TABLE [Order] ADD ShippingMethod NVARCHAR(50) NULL;
EXEC sp_rename '[Order].AddressDelivery', 'ShippingAddress', 'COLUMN';
ALTER TABLE OrderDetail ADD TotalPrice AS (Quantity * UnitPrice) PERSISTED;


