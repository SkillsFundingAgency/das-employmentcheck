﻿CREATE TABLE [dbo].[ApprenticeEmploymentCheckMessageQueue]
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
    CONSTRAINT UC_ApprenticeEmploymentCheckMessageQueue UNIQUE ([EmploymentCheckId], [Uln], [NationalInsuranceNumber], [PayeScheme], [StartDateTime], [EndDateTime])
)
GO
CREATE NONCLUSTERED INDEX [IX_ApprenticeEmploymentCheckMessageQueue__EmploymentCheckId] ON [dbo].[ApprenticeEmploymentCheckMessageQueue] ([EmploymentCheckId])
GO
