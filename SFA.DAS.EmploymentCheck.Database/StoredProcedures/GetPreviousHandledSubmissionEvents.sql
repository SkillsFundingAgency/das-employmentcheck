CREATE PROCEDURE [employer_check].[GetPreviousHandledSubmissionEvents]
	@json NVARCHAR(MAX)
AS

SELECT NiNumber, Uln, PassedValidationCheck
FROM [employer_check].[DAS_SubmissionEvents]
WHERE Uln IN (
		SELECT [value]
		FROM OPENJSON(@json)
	)