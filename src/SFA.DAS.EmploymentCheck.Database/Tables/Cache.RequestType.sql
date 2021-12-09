CREATE TABLE [Cache].[RequestType](
	[Id] TINYINT IDENTITY NOT NULL,
	[Description] [varchar](255) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL
 CONSTRAINT [PK_Cache_RequestType] PRIMARY KEY CLUSTERED
(
	[Id] ASC
))