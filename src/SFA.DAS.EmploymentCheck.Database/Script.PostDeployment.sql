 UPDATE [Business].[EmploymentCheck]
   SET [MessageSentDate] = '1-Jan-2022'
   WHERE [MessageSentDate] IS NULL
   AND [CreatedOn] < '11-May-2022';