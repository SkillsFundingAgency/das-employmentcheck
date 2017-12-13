CREATE TABLE [employer_check].[DAS_SubmissionEvents]
(
	[Id] BIGINT PRIMARY KEY,
	[NiNumber] NVARCHAR(9) NULL,
	[Uln] BIGINT NOT NULL,
	[PassedValidationCheck] BIT,
    [CreatedOn] DATE NOT NULL DEFAULT GetDate()
)