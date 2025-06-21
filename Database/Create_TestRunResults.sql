USE [AdoChecker]
GO
/****** Object:  Table [dbo].[TestRunResults]    Script Date: 6/2/2025 12:23:16 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TestRunResults](
	[RunId] [int] IDENTITY(1,1) NOT NULL,
	[RunDate] [datetime] NOT NULL,
	[ResultSummary] [nvarchar](max) NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[RunId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[TestRunResults] ADD  DEFAULT (getdate()) FOR [RunDate]
GO


