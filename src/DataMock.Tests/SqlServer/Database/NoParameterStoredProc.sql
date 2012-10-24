-- No Parameters A Calls B

CREATE PROCEDURE NoParametersProcA 
AS
BEGIN
	
	DECLARE @ResultTable TABLE
	( 
		Id int, 
		TextMessage VARCHAR(MAX)
	) 

	INSERT INTO @ResultTable(Id, TextMessage) 
	EXECUTE dbo.NoParametersProcB
	
	SELECT Id, TextMessage FROM @ResultTable
	UNION ALL
	SELECT 10, 'Message from Proc A'
	
END

GO

CREATE PROCEDURE NoParametersProcB 
AS
BEGIN
	SELECT 20, 'Message from Proc B'
END