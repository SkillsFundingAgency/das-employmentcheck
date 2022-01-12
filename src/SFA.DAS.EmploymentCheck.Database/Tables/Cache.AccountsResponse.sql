CREATE TABLE [Cache].[AccountsResponse](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ApprenticeEmploymentCheckId] [bigint] NOT NULL,
	[CorrelationId] [uniqueidentifier] NOT NULL,
	[AccountId] [bigint] NOT NULL,
	[PayeSchemes] [varchar](8000),
	[HttpResponse] [varchar](8000) NULL,
	[HttpStatusCode] [smallint] NULL,
	[CreatedOn] [datetime] NOT NULL,
	CONSTRAINT [PK_Cache_AccountsResponse] PRIMARY KEY CLUSTERED
	(
		[Id] ASC
	),
	CONSTRAINT UC_Cache_AccountsResponse UNIQUE ([ApprenticeEmploymentCheckId], [CorrelationId], [AccountId], [PayeSchemes])
)
GO