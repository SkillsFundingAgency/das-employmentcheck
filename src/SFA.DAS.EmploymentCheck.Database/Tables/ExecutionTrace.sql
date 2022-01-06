CREATE TABLE [dbo].[ExecutionTrace]
(
	[Timestamp] TIMESTAMP NOT NULL PRIMARY KEY,
	[Message] VARCHAR(500) NOT NULL
)
GO