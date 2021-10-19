CREATE PROCEDURE [dbo].[SaveEmploymentCheckResult]
	@id BIGINT,
	@result BIT
	
AS
INSERT INTO [SavedEmploymentCheckResults] (CheckId, Result) VALUES (@id, @result)
