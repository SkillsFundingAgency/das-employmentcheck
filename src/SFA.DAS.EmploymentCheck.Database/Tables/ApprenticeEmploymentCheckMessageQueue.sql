CREATE TABLE [dbo].[ApprenticeEmploymentCheckMessageQueue]
(
    [MessageId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [MessageCreatedDateTime] DATETIME NOT NULL,
    [EmploymentCheckId] BIGINT NOT NULL,
    [Uln] BIGINT NOT NULL,
    [NationalInsuranceNumber] VARCHAR(20) NULL,
    [PayeScheme] VARCHAR(255) NULL,
    [StartDateTime] DATETIME NOT NULL,
    [EndDateTime] DATETIME NOT NULL,
    [EmploymentCheckedDateTime] DATETIME NULL,
    [IsEmployed] BIT NULL,
    [ReturnCode] VARCHAR(50) NULL,
    [ReturnMessage] VARCHAR(MAX) NULL
)
GO
CREATE NONCLUSTERED INDEX [IX_ApprenticeEmploymentCheckMessageQueue__MessageId] ON [dbo].[ApprenticeEmploymentCheckMessageQueue] ([EmploymentCheckId])
INCLUDE ([PayeScheme]) WITH (ONLINE = ON)
GO