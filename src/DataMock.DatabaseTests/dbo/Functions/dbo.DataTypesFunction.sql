CREATE FUNCTION [dbo].[DataTypesFunction]
(
	@int INT,
	@string CHAR(5),
	@guid UNIQUEIDENTIFIER,
	@dateTime DATETIME
)
RETURNS @returntable TABLE
(
	c1 INT,
	c2 CHAR(5),
	c3 UNIQUEIDENTIFIER,
	c4 DATETIME
)
AS
BEGIN
	INSERT @returntable
	SELECT @int, @string, @guid, @dateTime
	RETURN
END
