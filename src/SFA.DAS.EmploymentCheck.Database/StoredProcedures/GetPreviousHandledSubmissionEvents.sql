CREATE PROCEDURE [employer_check].[GetPreviousHandledSubmissionEvents]
	@ulns [employer_check].[UlnTableType] READONLY
AS

SELECT NiNumber, Uln, PassedValidationCheck
FROM [employer_check].[DAS_SubmissionEvents]
WHERE Uln IN (SELECT Uln FROM @ulns)