CREATE TABLE [Cache].[EmploymentCheckMessageQueueHistory](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[MessageId] [bigint] NOT NULL,
	[EmploymentCheckId] [bigint] NOT NULL,
	[CorrelationId] UNIQUEIDENTIFIER NULL,
	[Uln] [bigint] NOT NULL,
	[NationalInsuranceNumber] [varchar](20) NOT NULL,
	[PayeScheme] [varchar](255) NOT NULL,
	[MinDateTime] [datetime] NOT NULL,
	[MaxDateTime] [datetime] NOT NULL,
	[Employed] [bit] NOT NULL,
	[LastEmploymentCheck] [datetime] NOT NULL,
	[ResponseHttpStatusCode] [smallint] NULL,
	[ResponseMessage] [varchar](MAX) NULL,
	[MessageCreatedOn] [datetime] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK__Employme__2E7B327711E54700] PRIMARY KEY CLUSTERED
(
	[Id] ASC
))