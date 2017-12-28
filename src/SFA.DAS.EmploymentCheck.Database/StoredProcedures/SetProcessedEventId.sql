CREATE PROCEDURE [employer_check].[SetLastProcessedEventId]
	@lastEventId BIGINT
AS

DELETE FROM [employer_check].[LastProcessedEvent]
INSERT INTO [employer_check].[LastProcessedEvent] (Id) VALUES (@lastEventId)

GO