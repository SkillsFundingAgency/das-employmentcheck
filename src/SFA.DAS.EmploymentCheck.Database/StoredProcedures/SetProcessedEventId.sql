CREATE PROCEDURE [employer_check].[SetLastProcessedEventId]
	@lastEventId BIGINT
AS

UPDATE [employer_check].[LastProcessedEvent] SET Id = @lastEventId

GO