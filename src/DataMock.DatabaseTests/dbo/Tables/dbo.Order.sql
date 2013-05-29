CREATE TABLE [dbo].[Order]
(
	[OrderId] INT IDENTITY NOT NULL PRIMARY KEY, 
    [Customer] VARCHAR(50) NOT NULL, 
    [DeliveryDate] DATETIME2 NULL, 
    [OrderDate] DATETIME2 NULL, 
    [OrderSequenceNumber] INT NULL
)
