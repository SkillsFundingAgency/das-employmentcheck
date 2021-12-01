CREATE TABLE [Business].[EmploymentCheckControlTable](
	[RowId] [bigint] NOT NULL,
	[EmploymentCheckLastHighestBatchId] [bigint] NOT NULL,
	CONSTRAINT [PK_EmploymentCheckControlTable] PRIMARY KEY CLUSTERED
	(
		[RowId] ASC
	)
)
GO