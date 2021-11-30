CREATE TABLE [Cache].[RefErrorType](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[Source] [varchar](50) NOT NULL,
	[Message] [varchar](50) NOT NULL,
	[MaxRetries] [bigint] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[LastUpdated] [datetime] NOT NULL,
	PRIMARY KEY CLUSTERED
	(
		[Id] ASC
	)
)