CREATE TABLE [dbo].[Parent]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] NVARCHAR(50) NOT NULL, 
    CONSTRAINT [CK_Parent_Name_NotEmpty] CHECK (Name <> '')
)
