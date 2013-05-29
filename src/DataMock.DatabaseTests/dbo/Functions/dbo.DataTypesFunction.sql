CREATE FUNCTION [dbo].[DataTypesFunction]
(
	@int INT,
	@string CHAR(5),
	@guid UNIQUEIDENTIFIER,
	@dateTime DATETIME,
	@boolean BIT
)
RETURNS @returntable TABLE
(
	c1 INT,
	c2 CHAR(5),
	c3 UNIQUEIDENTIFIER,
	c4 DATETIME,
	c5 BIT
)
AS
BEGIN
	INSERT @returntable
	SELECT @int, @string, @guid, @dateTime, @boolean
	RETURN
END
