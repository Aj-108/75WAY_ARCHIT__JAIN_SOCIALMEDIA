SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserDB](
	[user_id] [int] IDENTITY(1,1) NOT NULL,
	[username ] [varchar](max) NOT NULL,
	[email] [varchar](max) NOT NULL,
	[password] [nvarchar](max) NOT NULL,
	[followers] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[UserDB] ADD  CONSTRAINT [DEFAULT_UserDB_followers]  DEFAULT ((0)) FOR [followers]
GO
