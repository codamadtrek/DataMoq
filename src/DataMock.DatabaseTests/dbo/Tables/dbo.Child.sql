CREATE TABLE [dbo].[Child]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] NVARCHAR(50) NOT NULL, 
    [FavoriteSubject] NVARCHAR(50) NOT NULL, 
    [Parent] INT NOT NULL, 
    CONSTRAINT [FK_Child_Parent] FOREIGN KEY (Parent) REFERENCES Parent(Id),
	CONSTRAINT [CK_Child_Name_NotEmpty] CHECK (Name <> '')
)
