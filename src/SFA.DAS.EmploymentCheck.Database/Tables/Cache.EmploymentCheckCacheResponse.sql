CREATE TABLE [Cache].[EmploymentCheckCacheResponse](
	[Id] [bigint] NOT NULL,
	[CorrelationId] [bigint] NOT NULL,
	[Employed] [int] NULL,
	[FoundOnPaye] [varchar](255) NULL,
	[ProcessingComplete] [bit] NOT NULL,
	[Count] [int] NULL,
	[HmrcResponse] [varchar](1024) NULL,
	[CreatedOn] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_EmploymentCheckCacheResponse] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
))