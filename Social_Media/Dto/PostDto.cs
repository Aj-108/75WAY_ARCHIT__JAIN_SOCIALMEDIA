using System;
namespace Social_Media.Dto
{
	public class PostDto
	{
		public int PostId { get; set; }

		public int UserId { get; set; }

        public int Likes { get; set; }

        public int Comments { get; set; }


        public string? PostData { get; set; }

		public string? PostFilePath { get; set; }

		public DateTime PostCreatedAt { get; set; }

	}
}

