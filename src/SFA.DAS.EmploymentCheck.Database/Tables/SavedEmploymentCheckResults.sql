CREATE TABLE [dbo].[SavedEmploymentCheckResults]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [CheckId] BIGINT NOT NULL, 
    [Result] BIT NOT NULL
)
