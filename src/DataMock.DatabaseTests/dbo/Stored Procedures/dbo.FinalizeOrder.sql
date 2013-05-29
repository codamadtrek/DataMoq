CREATE PROCEDURE [dbo].[FinalizeOrder]
	@orderId int = 0
AS
	DECLARE @sequence INT = dbo.[GetNextOrderSequence]()

	UPDATE [Order]	
	SET
		OrderSequenceNumber = @sequence,
		OrderDate = GETDATE()
	OUTPUT @sequence AS Sequence
	WHERE OrderId = @orderId
RETURN
