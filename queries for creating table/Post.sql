SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Post](
	[post_id] [int] IDENTITY(100,6) NOT NULL,
	[user_id] [int] NOT NULL,
	[content] [varchar](max) NOT NULL,
	[filePath] [varchar](max) NOT NULL,
	[createdAt] [datetime] NOT NULL,
	[likes] [int] NULL,
	[comments] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[Post] ADD  CONSTRAINT [PK_Post] PRIMARY KEY CLUSTERED 
(
	[post_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Post] ADD  CONSTRAINT [DEFAULT_Post_likes]  DEFAULT ((0)) FOR [likes]
GO
ALTER TABLE [dbo].[Post] ADD  CONSTRAINT [DEFAULT_Post_comments]  DEFAULT ((0)) FOR [comments]
GO
