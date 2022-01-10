CREATE TABLE [Cache].[EmploymentCheckCacheRequest](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[EmploymentCheckId] [bigint] NOT NULL,
	[CorrelationId] [uniqueidentifier] NULL,
	[Nino] [varchar](20) NOT NULL,
	[PayeScheme] [varchar](500) NOT NULL,
	[MinDate] [datetime] NOT NULL,
	[MaxDate] [datetime] NOT NULL,
	[Employed] [bit] NULL,
	[RequestCompletionStatus] [smallint] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[LastUpdatedOn] [datetime] NULL,
	CONSTRAINT [PK_Cache_EmploymentCheckCacheRequest] PRIMARY KEY CLUSTERED
	(
		[Id] ASC
	)
)
GO