CREATE TABLE [dbo].[EmploymentChecks]
(
	[Id] BIGINT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [ULN] BIGINT NOT NULL,
    [UKPRN] BIGINT NULL,
    [ApprenticeshipId] BIGINT NULL,
    [AccountId] BIGINT NULL,
    [NINO] NCHAR(9) NOT NULL,
    [MinDate] DATETIME NOT NULL,
    [MaxDate] DATETIME NOT NULL,
    [CheckType] NVARCHAR(20) NOT NULL,
    [Result] BIT NULL,
    [LastUpdated] DATETIME NULL,
    [CreatedDate] DATETIME NULL DEFAULT getdate(),
    CONSTRAINT UC_EmploymentChecks UNIQUE (Accountid, uln, ukprn, apprenticeshipid, nino, checktype)
)
GO
CREATE INDEX [IX_EmploymentChecks_Column] ON [dbo].[EmploymentChecks] (uln, accountid, ukprn, checktype)
go
CREATE INDEX [IX_EmploymentChecks_Checks] ON [dbo].[EmploymentChecks] (checktype, uln)