CREATE FUNCTION [dbo].[ScalarFunctionString]
(
	@param1 int
)
RETURNS VARCHAR(20)
AS
BEGIN
	RETURN 'HELLO!'
END
