CREATE TABLE [employer_check].[DAS_SubmissionEvents]
(
	[Uln] BIGINT NOT NULL,
	[NiNumber] NVARCHAR(9) NULL,
	[PassedValidationCheck] BIT,
    [CheckedOn] DATETIME NOT NULL DEFAULT GetDate(),
	CONSTRAINT [PK__employer_check_DAS_SubmissionEvents] PRIMARY KEY CLUSTERED
	(
		[Uln] ASC
	)
)
GO