CREATE TABLE [Business].[EmploymentCheck](
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
	[CreatedOn] [datetime] NOT NULL,
	[LastUpdatedOn] [datetime] NULL,
	CONSTRAINT [PK_Business_EmploymentCheck] PRIMARY KEY CLUSTERED
	(
		[Id] ASC
	)
)
GO

CREATE NONCLUSTERED INDEX [IX_Business_EmploymentCheck__Id_RequestCompletionStatus_AccountId]
    ON [Business].[EmploymentCheck]([Id] ASC, [RequestCompletionStatus] ASC, [AccountId] ASC)
	INCLUDE([CreatedOn]) WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY];
GO