CREATE VIEW [dbo].[People]
	AS 
SELECT 
	Child.Name AS Child
	,Parent.Name AS Parent
FROM [Parent]
JOIN Child ON Child.Parent = Parent.Id

