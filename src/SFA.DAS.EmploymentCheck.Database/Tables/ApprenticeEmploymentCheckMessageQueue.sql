CREATE TABLE [dbo].[ApprenticeEmploymentCheckMessageQueue]
(
	[MessageId] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [MessageCreatedDateTime] DATETIME NOT NULL,
    [EmploymentCheckId] BIGINT NOT NULL,
	[Uln] BIGINT NOT NULL,
    [NationalInsuranceNumber] VARCHAR(20) NOT NULL,
    [PayeScheme] VARCHAR(255) NOT NULL,
    [StartDateTime] NCHAR(10) NOT NULL,
    [EndDateTime] NCHAR(10) NOT NULL,
    [EmploymentCheckedDateTime] DATETIME NULL,
    [IsEmployed] BIT NULL,
    [ReturnCode] VARCHAR(50) NULL,
    [ReturnMessage] VARCHAR(500) NULL
)
GO