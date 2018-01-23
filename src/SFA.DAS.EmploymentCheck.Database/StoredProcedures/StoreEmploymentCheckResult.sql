CREATE PROCEDURE [employer_check].[StoreEmploymentCheckResult]
	@uln BIGINT,
	@nationalInsuranceNumber NVARCHAR(9),
	@passedValidationCheck BIT
AS
	MERGE [employer_check].[DAS_SubmissionEvents] AS [Target]
	USING (SELECT @uln AS Uln, @nationalInsuranceNumber AS NiNumber, @passedValidationCheck AS PassedValidationCheck) AS [Source] ON [Target].Uln = [Source].Uln
	WHEN MATCHED THEN UPDATE SET [Target].PassedValidationCheck = [Source].PassedValidationCheck, [Target].CheckedOn = GETDATE(), [Target].NiNumber = [Source].NiNumber
	WHEN NOT MATCHED THEN INSERT (Uln, NiNumber, PassedValidationCheck, CheckedOn) VALUES ([Source].Uln, [Source].NiNumber, [Source].PassedValidationCheck, GETDATE());
