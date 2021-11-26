CREATE TABLE [Cache].[EmploymentCheckMessageQueueHistory](
	[MessageHistoryId] [bigint] IDENTITY(1,1) NOT NULL,
	[MessageId] [bigint] NOT NULL,
	[EmploymentCheckId] [bigint] NOT NULL,
	[CorrelationId] [bigint] NOT NULL,
	[Uln] [bigint] NOT NULL,
	[NationalInsuranceNumber] [varchar](20) NOT NULL,
	[PayeScheme] [varchar](255) NOT NULL,
	[MinDateTime] [datetime] NOT NULL,
	[MaxDateTime] [datetime] NOT NULL,
	[IsEmployed] [bit] NOT NULL,
	[EmploymentCheckedDateTime] [datetime] NOT NULL,
	[ResponseId] [smallint] NULL,
	[ResponseMessage] [varchar](1024) NULL,
	[MessageCreatedDateTime] [datetime] NOT NULL,
	[CreatedDateTime] [datetime] NOT NULL,
 CONSTRAINT [PK__Employme__2E7B327711E54700] PRIMARY KEY CLUSTERED
(
	[MessageHistoryId] ASC
))