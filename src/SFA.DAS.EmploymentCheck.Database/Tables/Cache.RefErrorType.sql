CREATE TABLE [Cache].[RefErrorType](
	[RefErrorTypeId] [bigint] IDENTITY(1,1) NOT NULL,
	[Message] [varchar](50) NOT NULL,
	[MaxRetries] [bigint] NOT NULL,
	[LastUpdatedDate] [datetime] NULL,
	[CreatedDate] [datetime] NULL,
	PRIMARY KEY CLUSTERED
	(
		[RefErrorTypeId] ASC
	)
)