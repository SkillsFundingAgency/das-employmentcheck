CREATE TABLE [Cache].[DataCollectionsResponse](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[EmploymentCheckId] [bigint] NOT NULL,
	[CorrelationId] [uniqueidentifier] NULL,
	[Uln] [bigint] NOT NULL,
	[NiNumber] [varchar](20) NULL,
	[HttpResponse] [varchar](2000) NULL,
	[HttpStatusCode] [smallint] NULL,
	[CreatedOn] [datetime] NOT NULL,
	CONSTRAINT [PK_Cache_DataCollectionsResponse] PRIMARY KEY CLUSTERED
	(
		[Id] ASC
	)
)
GO
