CREATE FUNCTION [dbo].[GetNextOrderSequence]
(	
)
RETURNS INT
AS
BEGIN
	RETURN COALESCE((SELECT MAX(OrderSequenceNumber) FROM [Order]) + 1, 0)
END
