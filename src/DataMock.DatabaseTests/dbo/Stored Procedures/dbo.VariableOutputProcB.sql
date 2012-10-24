CREATE PROCEDURE VariableOutputProcB 
	@index INT
AS
BEGIN
	SELECT @index+ 100 Id, 
		CAST((SELECT [text] FROM sys.messages 
		 WHERE message_id = (100 + @index)
			AND language_id = 3082) AS VARCHAR(MAX)) TextMessage
END