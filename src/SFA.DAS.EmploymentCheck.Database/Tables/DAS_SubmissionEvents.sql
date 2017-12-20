CREATE TABLE [employer_check].[DAS_SubmissionEvents]
(
	[Id] BIGINT PRIMARY KEY,
	[NiNumber] NVARCHAR(9) NULL,
	[Uln] BIGINT NOT NULL,
	[PassedValidationCheck] BIT,
    [CreatedOn] DATE NOT NULL DEFAULT GetDate()
)
GO
CREATE INDEX IX_SubmissionEvent_Uln ON [employer_check].[DAS_SubmissionEvents] (Uln) INCLUDE ([NiNumber], [PassedValidationCheck])