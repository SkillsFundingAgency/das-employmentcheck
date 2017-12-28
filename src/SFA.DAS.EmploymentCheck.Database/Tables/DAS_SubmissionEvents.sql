CREATE TABLE [employer_check].[DAS_SubmissionEvents]
(
	[Uln] BIGINT NOT NULL PRIMARY KEY,
	[NiNumber] NVARCHAR(9) NULL,
	[PassedValidationCheck] BIT,
    [CheckedOn] DATE NOT NULL DEFAULT GetDate()
)
GO