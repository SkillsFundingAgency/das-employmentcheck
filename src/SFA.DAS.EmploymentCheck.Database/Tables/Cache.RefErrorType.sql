CREATE TABLE [Cache].[RefErrorType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmploymentCheckId] [bigint] NOT NULL,
	[Source] [varchar](max) NOT NULL,
	[Message] [varchar](max) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[LastUpdatedOn] [datetime] NOT NULL,
	CONSTRAINT [PK_Cache_RefErrorType] PRIMARY KEY CLUSTERED
	(
		[Id] ASC
	)
)
GO
