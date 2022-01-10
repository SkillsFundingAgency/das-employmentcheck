CREATE TABLE [Cache].[EmploymentCheckCacheResponse](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[EmploymentCheckId] [bigint] NOT NULL,
	[EmploymentCheckCacheRequestId] [bigint] NOT NULL,
	[CorrelationId] [uniqueidentifier] NULL,
	[Employed] [bit] NULL,
	[FoundOnPaye] [varchar](2000) NULL,
	[ProcessingComplete][bit] NOT NULL,
	[Count] [int] NOT NULL,
	[HttpResponse] [varchar](2000) NULL,
	[HttpStatusCode] [smallint] NULL,
	[CreatedOn] [datetime] NOT NULL,
	CONSTRAINT [PK_Cache_EmploymentCheckCacheResponse] PRIMARY KEY CLUSTERED
	(
		[Id] ASC
	)
)
GO