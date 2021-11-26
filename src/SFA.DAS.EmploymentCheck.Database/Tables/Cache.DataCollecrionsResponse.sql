CREATE TABLE [Cache].[DataCollectionsResponse](
	[Id] [bigint] IDENTITY NOT NULL,
	[CorrelationId] [bigint] NOT NULL,
	[Uln] [bigint] NOT NULL,
	[NiNumber] [varchar](20) NULL,
	[Response] [varchar](1024) NULL,
	[CreatedOn] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_DataCollectionsResponse] PRIMARY KEY CLUSTERED
(
	[Id] ASC
))