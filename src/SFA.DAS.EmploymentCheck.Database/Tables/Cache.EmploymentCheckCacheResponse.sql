CREATE TABLE [Cache].[EmploymentCheckCacheResponse](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[ApprenticeEmploymentCheckId] [bigint] NOT NULL,
	[EmploymentCheckCacheRequestId] [bigint] NOT NULL,
	[CorrelationId] [uniqueidentifier] NOT NULL,
	[Employed] [bit] NULL,
	[FoundOnPaye] [varchar](8000) NULL,
	[ProcessingComplete][bit] NOT NULL,
	[Count] [int] NOT NULL,
	[HttpResponse] [varchar](8000) NULL,
	[HttpStatusCode] [smallint] NULL,
	[CreatedOn] [datetime] NOT NULL,
	CONSTRAINT [PK_Cache_EmploymentCheckCacheResponse] PRIMARY KEY CLUSTERED
	(
		[Id] ASC
	),
	CONSTRAINT UC_Cache_EmploymentCheckCacheResponse UNIQUE ([ApprenticeEmploymentCheckId], [EmploymentCheckCacheRequestId], [CorrelationId], [FoundOnPaye])
)
GO