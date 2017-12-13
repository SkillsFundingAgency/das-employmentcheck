CREATE PROCEDURE [employer_check].[GetLastKnownProcessedEventId]
AS

SELECT MAX([Id]) AS Id FROM [employer_check].[DAS_SubmissionEvents]

GO