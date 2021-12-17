CREATE TABLE [dbo].[EmploymentChecks]
(
    [Id] BIGINT NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [ULN] BIGINT NOT NULL,
    [NationalInsuranceNumber] VARCHAR(20),
    [UKPRN] BIGINT NULL,
    [ApprenticeshipId] BIGINT NULL,
    [AccountId] BIGINT NULL,
    [MinDate] DATETIME NOT NULL,
    [MaxDate] DATETIME NOT NULL,
    [CheckType] NVARCHAR(20) NOT NULL,
    [IsEmployed] BIT NULL,
    [LastUpdated] DATETIME NULL,
    [CreatedDate] DATETIME NULL DEFAULT getdate(),
    [HasBeenChecked] BIT NOT NULL DEFAULT 0,
    [ReturnCode] VARCHAR(50) NULL,
    [ReturnMessage] VARCHAR(MAX) NULL
    CONSTRAINT UC_EmploymentChecks UNIQUE ([AccountId], [ULN], [UKPRN], [ApprenticeshipId], [CheckType])
)
GO
CREATE INDEX [IX_EmploymentChecks_Column] ON [dbo].[EmploymentChecks] ([ULN], [AccountId], [UKPRN], [CheckType])
GO
CREATE INDEX [IX_EmploymentChecks_Checks] ON [dbo].[EmploymentChecks] ([CheckType], [ULN])
GO
CREATE NONCLUSTERED INDEX [CREATE NONCLUSTERED INDEX [IX_EmploymentChecks__Id_HasBeenChecked] ON [dbo].[EmploymentChecks] ([HasBeenChecked], [Id]) 
    INCLUDE ([AccountId], [ApprenticeshipId], [CheckType], [CreatedDate], [IsEmployed], [LastUpdated], [MaxDate], [MinDate], [UKPRN], [ULN]) WITH (ONLINE = ON)
GO
