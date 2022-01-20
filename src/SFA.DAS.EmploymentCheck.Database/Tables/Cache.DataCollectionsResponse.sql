CREATE TABLE [Cache].[DataCollectionsResponse](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ApprenticeEmploymentCheckId] [bigint] NOT NULL,
	[CorrelationId] [uniqueidentifier] NOT NULL,
	[Uln] [bigint] NOT NULL,
	[NiNumber] [varchar](20) NULL,
	[HttpResponse] [varchar](8000) NULL,
	[HttpStatusCode] [smallint] NULL,
	[CreatedOn] [datetime] NOT NULL,
	CONSTRAINT [PK_Cache_DataCollectionsResponse] PRIMARY KEY CLUSTERED
	(
		[Id] ASC
	),
	CONSTRAINT UC_Cache_DataCollectionsResponse UNIQUE ([ApprenticeEmploymentCheckId], [CorrelationId], [Uln], [NiNumber])
)
GO
