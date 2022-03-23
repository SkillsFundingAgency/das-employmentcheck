CREATE TABLE [Cache].[AccountsResponse](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ApprenticeEmploymentCheckId] [bigint] NOT NULL,
	[CorrelationId] [uniqueidentifier] NOT NULL,
	[AccountId] [bigint] NOT NULL,
	[PayeSchemes] [varchar](max) NULL,
	[HttpResponse] [varchar](max) NULL,
	[HttpStatusCode] [smallint] NULL,
	[CreatedOn] [datetime] NOT NULL,
	[LastUpdatedOn] [datetime] NULL,
	CONSTRAINT [PK_Cache_AccountsResponse] PRIMARY KEY CLUSTERED
	(
		[Id] ASC
	)
)
GO

