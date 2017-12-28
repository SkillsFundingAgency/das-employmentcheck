CREATE PROCEDURE [employer_check].[GetLastKnownProcessedEventId]
AS

SELECT Id FROM [employer_check].[LastProcessedEvent]

GO