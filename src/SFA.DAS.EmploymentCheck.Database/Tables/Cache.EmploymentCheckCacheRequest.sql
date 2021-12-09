CREATE TABLE [Cache].[EmploymentCheckCacheRequest](
	[Id] [bigint] IDENTITY NOT NULL,
	[EmploymentCheckId] [bigint] NOT NULL,
	[RequestTypeId] [tinyint] NOT NULL,
	[CorrelationId] UNIQUEIDENTIFIER NULL,
	[Description] [varchar](255),
	[Data] [varchar](MAX),
	[CreatedOn] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_Cache_EmploymentCheckCacheRequest] PRIMARY KEY CLUSTERED
(
	[Id] ASC
))