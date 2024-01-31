using System;
namespace Social_Media.Dto
{
	public class CommentDto
	{
		public int UserId { get; set; }

		public string CommentText { get; set; }

		public DateTime CommentedAt { get; set; }

	}
}

