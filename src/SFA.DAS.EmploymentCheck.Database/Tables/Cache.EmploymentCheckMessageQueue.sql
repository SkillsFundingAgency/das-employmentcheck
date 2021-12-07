CREATE TABLE [Cache].[EmploymentCheckMessageQueue](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[EmploymentCheckId] [bigint] NOT NULL,
	[CorrelationId] UNIQUEIDENTIFIER NULL,
	[Uln] [bigint] NOT NULL,
	[NationalInsuranceNumber] [varchar](20) NOT NULL,
	[PayeScheme] [varchar](255) NOT NULL,
	[MinDateTime] [datetime] NOT NULL,
	[MaxDateTime] [datetime] NOT NULL,
	[Employed] [bit] NULL,
	[LastEmploymentCheck] [datetime] NULL,
	[ResponseHttpStatusCode] [smallint] NULL,
	[ResponseMessage] NVARCHAR(MAX) NULL,
	[CreatedOn] [datetime] NOT NULL
PRIMARY KEY CLUSTERED
(
	[Id] ASC
))