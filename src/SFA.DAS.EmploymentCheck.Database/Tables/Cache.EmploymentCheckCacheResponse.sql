CREATE TABLE [Cache].[EmploymentCheckCacheResponse](
	[Id] [bigint] NOT NULL,
	[EmploymentCheckCacheRequestId] [bigint] NOT NULL,
	[CorrelationId] UNIQUEIDENTIFIER NULL,
	[Employed] [int] NULL,
	[FoundOnPaye] [varchar](255) NULL,
	[ProcessingComplete] [bit] NOT NULL,
	[Count] [int] NULL,
	[Response] [varchar](1024) NULL,
	[CreatedOn] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_Cache_EmploymentCheckCacheResponse] PRIMARY KEY CLUSTERED
(
	[Id] ASC
))