USE [AdoChecker]
GO

/****** Object:  Table [dbo].[TestRunDetails]    Script Date: 6/2/2025 12:22:43 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TestRunDetails](
	[DetailId] [int] IDENTITY(1,1) NOT NULL,
	[RunId] [int] NOT NULL,
	[AdoItemId] [nvarchar](100) NULL,
	[CallReference] [nvarchar](max) NULL,
	[ImpactAssessment] [nvarchar](50) NULL,
	[RootCauseAnalysis] [nvarchar](20) NULL,
	[ProjectZero] [nvarchar](20) NULL,
	[PRLifecycle] [nvarchar](20) NULL,
	[StatusDiscrepancy] [nvarchar](10) NULL,
	[TestCaseGap] [nvarchar](200) NULL,
	[CurrentStatus] [nvarchar](20) NULL,
	[TechnicalLeadName] [nvarchar](100) NULL,
	[DevName] [nvarchar](50) NULL,
	[WorkitemType] [nvarchar](20) NULL,
PRIMARY KEY CLUSTERED 
(
	[DetailId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[TestRunDetails]  WITH CHECK ADD  CONSTRAINT [FK_TestRunDetails_RunId] FOREIGN KEY([RunId])
REFERENCES [dbo].[TestRunResults] ([RunId])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[TestRunDetails] CHECK CONSTRAINT [FK_TestRunDetails_RunId]
GO


