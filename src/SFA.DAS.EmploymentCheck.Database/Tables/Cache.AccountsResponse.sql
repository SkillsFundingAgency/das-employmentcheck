CREATE TABLE [Cache].[AccountsResponse](
	[Id] [bigint] IDENTITY NOT NULL,
	[CorrelationId] [bigint] NOT NULL,
	[AccountId] [bigint] NOT NULL,
	[PayeSchemes] [varchar](1024) NULL,
	[Response] [varchar](1024) NULL,
	[CreatedOn] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
 CONSTRAINT [PK_AccountsResponse] PRIMARY KEY CLUSTERED
(
	[Id] ASC
))