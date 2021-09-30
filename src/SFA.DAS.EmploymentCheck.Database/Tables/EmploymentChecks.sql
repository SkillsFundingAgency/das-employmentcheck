CREATE TABLE [dbo].[EmploymentChecks]
(
	[Id] BIGINT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [ULN] BIGINT NOT NULL,
    [UKPRN] BIGINT NULL,
    [ApprenticeshipId] BIGINT NULL,
    [AccountId] BIGINT NULL,
    [NationalInsuranceNumber] NCHAR(9) NOT NULL,
    [MinDate] DATETIME NOT NULL,
    [MaxDate] DATETIME NOT NULL,
    [CheckType] NVARCHAR(20) NOT NULL,
    [IsEmployed] BIT NULL,
    [LastUpdated] DATETIME NULL,
    [CreatedDate] DATETIME NULL DEFAULT getdate(),
    [HasBeenChecked] BIT NOT NULL DEFAULT 0, 
    CONSTRAINT UC_EmploymentChecks UNIQUE (Accountid, uln, ukprn, apprenticeshipid, nationalinsurancenumber, checktype)
)
GO
CREATE INDEX [IX_EmploymentChecks_Column] ON [dbo].[EmploymentChecks] (uln, accountid, ukprn, checktype)
go
CREATE INDEX [IX_EmploymentChecks_Checks] ON [dbo].[EmploymentChecks] (checktype, uln)