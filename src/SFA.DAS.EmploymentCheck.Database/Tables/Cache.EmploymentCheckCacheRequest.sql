CREATE TABLE [Cache].[EmploymentCheckCacheRequest](
	[Id] [bigint] IDENTITY NOT NULL,
	[CorrelationId] [bigint] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_EmploymentCheckCacheRequest] PRIMARY KEY CLUSTERED
(
	[Id] ASC
))