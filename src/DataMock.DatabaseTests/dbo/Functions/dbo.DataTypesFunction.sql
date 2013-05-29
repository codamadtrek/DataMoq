CREATE FUNCTION [dbo].[DataTypesFunction]
(
	@int INT,
	@string CHAR(5),
	@guid UNIQUEIDENTIFIER,
	@dateTime DATETIME,
	@boolean BIT,
	@char CHAR
)
RETURNS @returntable TABLE
(
	c1 INT,
	c2 CHAR(5),
	c3 UNIQUEIDENTIFIER,
	c4 DATETIME,
	c5 BIT,
	c6 CHAR
)
AS
BEGIN
	INSERT @returntable
	SELECT @int, @string, @guid, @dateTime, @boolean, @char
	RETURN
END
