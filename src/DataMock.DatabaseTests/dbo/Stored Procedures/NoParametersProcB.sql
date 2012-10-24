CREATE PROCEDURE [dbo].[NoParametersProcB] 
AS
BEGIN
	SELECT 20 AS ID, CAST('Message from Proc B' AS VARCHAR(MAX)) AS TextMessage
END
