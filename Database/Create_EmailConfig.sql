USE [AdoChecker]
GO
DROP TABLE IF EXISTS [dbo].[EmailConfig];
/****** Object:  Table [dbo].[EmailConfig]    Script Date: 6/2/2025 12:22:43 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[EmailConfig](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EmailId] [nvarchar](max) NULL,
	[CreatedDate] DateTime NULL,
	[ModifiedDate] DateTime NULL,
	[IsActive] bit Default(1),
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


