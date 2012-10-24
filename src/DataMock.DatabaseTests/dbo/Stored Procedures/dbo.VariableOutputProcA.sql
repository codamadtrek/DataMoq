CREATE PROCEDURE VariableOutputProcA 
	@count int
AS
BEGIN
	
	DECLARE @ResultTable TABLE
	( 
		Id int, 
		TextMessage VARCHAR(MAX)
	) 
	
	DECLARE @index INT = 0
	
	WHILE( @index < @count )
	BEGIN
	
		SET @index = @index + 1
	
		INSERT INTO @ResultTable(Id, TextMessage) 
		EXECUTE dbo.VariableOutputProcB @index
	END

	SELECT * FROM @ResultTable
END