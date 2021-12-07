CREATE TABLE [Business].[EmploymentCheck]
(
	[Id] BIGINT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[CorrelationId] uniqueidentifier NOT NULL,
    [CheckType] VARCHAR(50) NOT NULL,
    [Uln] BIGINT NOT NULL,
    [ApprenticeshipId] BIGINT NULL,
    [AccountId] BIGINT NULL,
    [MinDate] DATETIME NOT NULL,
    [MaxDate] DATETIME NOT NULL,
    [Employed] BIT NULL,
    [CreatedOn] DATETIME NOT NULL,
    [LastUpdated] DATETIME NOT NULL DEFAULT getdate()
)
GO
CREATE INDEX [IX_EmploymentCheck_Uln_AccountId_CheckType] ON [Business].[EmploymentCheck] (Uln, AccountId, CheckType)
GO
CREATE INDEX [IX_EmploymentCheck_CheckType_Uln] ON [Business].[EmploymentCheck] (CheckType, Uln)
GO