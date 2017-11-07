CREATE TABLE [employer_check].[DAS_SubmissionEvents]
(
	[Id] BIGINT PRIMARY KEY,
	[NiNumber] NVARCHAR(9) NULL,
	[Uln] NVARCHAR(9) NULL,
	[PassedValidationCheck] BIT,
    [CreatedOn] DATE NOT NULL DEFAULT GetDate()
)