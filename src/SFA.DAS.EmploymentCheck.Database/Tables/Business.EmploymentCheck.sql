CREATE TABLE [Business].[EmploymentCheck]
(
	[EmploymentCheckId] BIGINT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[CorrelationId] BIGINT NOT NULL,
    [CheckType] VARCHAR(50) NOT NULL,
    [Uln] BIGINT NOT NULL,
    [ApprenticeshipId] BIGINT NULL,
    [AccountId] BIGINT NULL,
    [MinDate] DATETIME NOT NULL,
    [MaxDate] DATETIME NOT NULL,
    [IsEmployed] BIT NULL,
    [LastUpdated] DATETIME NULL DEFAULT getdate(),
    [CreatedOn] DATETIME NULL
)
GO
CREATE INDEX [IX_EmploymentCheck_Uln_AccountId_CheckType] ON [Business].[EmploymentCheck] (Uln, AccountId, CheckType)
GO
CREATE INDEX [IX_EmploymentCheck_CheckType_Uln] ON [Business].[EmploymentCheck] (CheckType, Uln)
GO