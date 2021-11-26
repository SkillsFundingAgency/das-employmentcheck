CREATE TABLE [Cache].[EmploymentCheckMessageQueue](
	[MessageId] [bigint] NOT NULL,
	[EmploymentCheckId] [bigint] NOT NULL,
	[CorrelationId] [bigint] NOT NULL,
	[Uln] [bigint] NOT NULL,
	[NationalInsuranceNumber] [varchar](20) NOT NULL,
	[PayeScheme] [varchar](255) NOT NULL,
	[MinDateTime] [datetime] NOT NULL,
	[MaxDateTime] [datetime] NOT NULL,
	[IsEmployed] [bit] NULL,
	[EmploymentCheckedDateTime] [datetime] NULL,
	[ResponseId] [smallint] NULL,
	[ResponseMessage] [varchar](1024) NULL,
	[CreatedDateTime] [datetime] NOT NULL
PRIMARY KEY CLUSTERED
(
	[MessageId] ASC
))