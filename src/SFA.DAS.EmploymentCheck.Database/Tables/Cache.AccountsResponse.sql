CREATE TABLE [Cache].[AccountsResponse](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[EmploymentCheckId] [bigint] NOT NULL,
	[CorrelationId] [uniqueidentifier] NULL,
	[AccountId] [bigint] NOT NULL,
	[PayeSchemes] [varchar](max),
	[HttpResponse] [varchar](2000) NULL,
	[HttpStatusCode] [smallint] NULL,
	[CreatedOn] [datetime] NOT NULL,
	CONSTRAINT [PK_Cache_AccountsResponse] PRIMARY KEY CLUSTERED
	(
		[Id] ASC
	)
)
GO