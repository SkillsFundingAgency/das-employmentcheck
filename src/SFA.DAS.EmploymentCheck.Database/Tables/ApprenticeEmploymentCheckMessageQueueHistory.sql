CREATE TABLE [dbo].[ApprenticeEmploymentCheckMessageQueueHistory]
(
    [MessageHistoryId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [MessageHistoryCreatedDateTime] DATETIME NOT NULL,
    [MessageId] UNIQUEIDENTIFIER NOT NULL,
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
