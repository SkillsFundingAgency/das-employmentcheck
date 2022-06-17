﻿CREATE TABLE [Business].[EmploymentCheck](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[CorrelationId] [uniqueidentifier] NOT NULL,
	[CheckType] [varchar](50) NOT NULL,
	[Uln] [bigint] NOT NULL,
	[ApprenticeshipId] [bigint] NULL,
	[AccountId] [bigint] NOT NULL,
	[MinDate] [datetime] NOT NULL,
	[MaxDate] [datetime] NOT NULL,
	[Employed] [bit] NULL,
	[RequestCompletionStatus] [smallint] NULL,
	[ErrorType] [nvarchar](50) NULL,
	[MessageSentDate] [datetime] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[LastUpdatedOn] [datetime] NULL,
	CONSTRAINT [PK_Business_EmploymentCheck] PRIMARY KEY CLUSTERED
	(
		[Id] ASC
	)
)
GO

CREATE NONCLUSTERED INDEX [Idx_Business_EmploymentCheck_RequestCompletionStatus_MessageSentDate] ON [Business].[EmploymentCheck]
(
	[RequestCompletionStatus] ASC,
	[MessageSentDate] ASC
)
GO