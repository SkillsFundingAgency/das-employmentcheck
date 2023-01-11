CREATE TABLE [Cache].[EmploymentCheckCacheRequest](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ApprenticeEmploymentCheckId] [bigint] NOT NULL,
	[CorrelationId] [uniqueidentifier] NOT NULL,
	[Nino] [varchar](20) NOT NULL,
	[PayeScheme] [varchar](1000) NOT NULL,
	[PayeSchemePriority] [int] NULL,
	[MinDate] [datetime] NOT NULL,
	[MaxDate] [datetime] NOT NULL,
	[Employed] [bit] NULL,
	[RequestCompletionStatus] [smallint] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[LastUpdatedOn] [datetime] NULL,
	CONSTRAINT [PK_Cache_EmploymentCheckCacheRequest] PRIMARY KEY CLUSTERED
	(
		[Id] ASC
	),
	CONSTRAINT UC_Cache_EmploymentCheckCacheRequest UNIQUE ([ApprenticeEmploymentCheckId], [CorrelationId], [Nino], [PayeScheme], [MinDate], [MaxDate])
)
GO

CREATE INDEX [IX_EmploymentCheckCacheRequest_Column] ON [Cache].[EmploymentCheckCacheRequest] ([ApprenticeEmploymentCheckId], [CorrelationId], [Nino], [PayeScheme], [MinDate], [MaxDate])
GO

CREATE NONCLUSTERED INDEX [IX_Cache_EmploymentCheckCacheRequest__ApprenticeEmploymentCheckId]
    ON [Cache].[EmploymentCheckCacheRequest]([ApprenticeEmploymentCheckId] ASC);
GO

CREATE INDEX IX_EmploymentCheckCacheRequest__RequestCompletionStatus_Id ON [Cache].[EmploymentCheckCacheRequest] ([RequestCompletionStatus], [Id])
GO

CREATE NONCLUSTERED INDEX [Get_Pending_EmploymentCheck_Requests] ON [Cache].[EmploymentCheckCacheRequest] 
	([RequestCompletionStatus]) 
INCLUDE 
	([ApprenticeEmploymentCheckId])
GO

CREATE NONCLUSTERED INDEX [Get_Learner_Paye_Check_Priority]
ON [Cache].[EmploymentCheckCacheRequest] ([Nino],[Employed])
INCLUDE ([PayeScheme],[CreatedOn])
GO
