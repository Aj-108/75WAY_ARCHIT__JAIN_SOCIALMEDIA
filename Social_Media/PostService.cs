using System;
using System.Data;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Social_Media.Dto;
using static System.Net.Mime.MediaTypeNames;

namespace Social_Media
{
	public interface IPostService
	{
        public Task<ResponseDto> CreatePost(PostDto postData,string userId,string userName);

        public Task<ResponseDto> LikePost(int postId, int userId);

        public Task<ResponseDto> CommentPost(int postId, int userId, string comment);

        public Task<List<CommentDto>> GetComments(int postId);

        public Task<List<PostDto>> GetPosts(int userId);

        public Task<List<PostDto>> GetTopPosts();
    }

    public class PostService : IPostService
	{
        public IDbConnection Db { get; set; }

        public PostService(IDbConnection _db)
		{
			Db = _db;
		}

		public async Task<ResponseDto> CreatePost(PostDto postData,string userId,string userName)
		{
			ResponseDto result = new()
			{
				Status = false,
				Message = "",
				Data = ""
			};

			try
            {
				string postFilePath = SavePostFile(userId, userName);
                using (var con = new SqlConnection(Db.GetConnectionString()))
                {
                    await con.OpenAsync();
                    StringBuilder sql = new StringBuilder();
                    sql.Append("INSERT INTO POST(user_id,content,filePath,createdAt,likes,comments) ");
                    sql.Append("VALUES(@userId,@postData,@postFilePath,@postCreatedAt,0,0);");
                    using (var cmd = new SqlCommand(sql.ToString(), con))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@userId", postData.UserId);
                        cmd.Parameters.AddWithValue("@postData", postData.PostData);
                        cmd.Parameters.AddWithValue("@postFilePath", postFilePath);
                        cmd.Parameters.AddWithValue("@postCreatedAt", postData.PostCreatedAt);
                        cmd.ExecuteNonQuery();
                        result.Status = true;
                        result.Message = "Post Created Successfully";
                        return result;
                    }
                }
            }
            catch (Exception ex)
			{
                throw new Exception(ex.Message);
            }
		}


        public async Task<ResponseDto> LikePost(int postId,int userId)
        {
            ResponseDto result = new()
            {
                Status = false ,
                Message = ""
            };

            try
            {
                using(var con=new SqlConnection(Db.GetConnectionString()))
                {
                    await con.OpenAsync();

                    // Checking if the post exists or not
                    string sql = "Select count(*) from Post where post_id = @postId";
                    using(var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@postid", postId);
                        int postExist = (int)await cmd.ExecuteScalarAsync();
                        if (postExist <= 0)
                        {
                            result.Message = "No Such post Exists";
                            return result;
                        }
                    }

          

                    //Liking post
                    DateTime today = DateTime.Now;
                    sql = "INSERT INTO LIKES(POST_ID,USER_ID,LIKED_AT) VALUES (@postId,@userId,@likedAt)";
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@postid", postId);
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@likedAt", today);
                        cmd.ExecuteNonQuery();
                        
                    }


                    sql = "UPDATE POST SET LIKES = LIKES+1  WHERE POST_ID = @postId AND USER_ID = @userId";
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@postid", postId);
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.ExecuteNonQuery();
                        result.Status = true;
                        result.Message = "Post Liked Successfully";
                        return result;
                    }

                   
                }
            }
            catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }


        public async Task<ResponseDto> CommentPost(int postId, int userId, string comment)
        {
            ResponseDto result = new()
            {
                Status = false,
                Message = ""
            };

            try
            {

                using(var con =new  SqlConnection(Db.GetConnectionString()))
                {
                    await con.OpenAsync();

                    // Checking if the post exists or not
                    string sql = "Select count(*) from Post where post_id = @postId";
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@postid", postId);
                        int postExist = (int)await cmd.ExecuteScalarAsync();
                        if (postExist <= 0)
                        {
                            result.Message = "No Such post Exists";
                            return result;
                        }
                    }

                    DateTime today = DateTime.Now;
                    sql = "INSERT INTO COMMENTS(POST_ID,USER_ID,COMMENT_TEXT,COMMENTED_AT) VALUES (@postId,@userId,@comment,@commentedAt)";
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@postid", postId);
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@comment", comment);
                        cmd.Parameters.AddWithValue("@commentedAt", today);
                        cmd.ExecuteNonQuery();
                    }

                    sql = "UPDATE POST SET COMMENTS = COMMENTS+1  WHERE POST_ID = @postId AND USER_ID = @userId";
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@postid", postId);
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.ExecuteNonQuery();
                        result.Status = true;
                        result.Message = "Post Liked Successfully";
                        return result;
                    }
                }


            }catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<List<CommentDto>> GetComments(int postId)
        {
            List<CommentDto> commentsList = new List<CommentDto>();
            try
            {

                using (var con = new SqlConnection(Db.GetConnectionString()))
                {
                    await con.OpenAsync();

                    CommentDto[] comments = new CommentDto[] { };
                    string sql = "SELECT * FROM COMMENTS WHERE POST_ID = @postId";
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@postid", postId);
                        //var sr = await cmd.ExecuteReaderAsync();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                CommentDto comment = new CommentDto
                                {
                                    UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
                                    CommentText = reader.GetString(reader.GetOrdinal("comment_text")),
                                    CommentedAt = reader.GetDateTime(reader.GetOrdinal("commented_at"))
                                };

                                commentsList.Add(comment);
                            }
                            return commentsList;
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<List<PostDto>> GetPosts (int userId)
        {
            List<PostDto> postsList = new List<PostDto>();
            try
            {

                using (var con = new SqlConnection(Db.GetConnectionString()))
                {
                    await con.OpenAsync();

                    string sql = "SELECT * FROM POST WHERE USER_ID = @userId";
                    using (var cmd = new SqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        //var sr = await cmd.ExecuteReaderAsync();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                PostDto comment = new PostDto
                                {
                                    UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
                                    PostId = reader.GetInt32(reader.GetOrdinal("post_id")),
                                    Likes = reader.GetInt32(reader.GetOrdinal("likes")),
                                    Comments = reader.GetInt32(reader.GetOrdinal("comments")),
                                    PostData = reader.GetString(reader.GetOrdinal("content")),
                                    PostCreatedAt = reader.GetDateTime(reader.GetOrdinal("createdAt"))
                                };

                                postsList.Add(comment);
                            }
                            return postsList;
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<PostDto>> GetTopPosts()
        {
            List<PostDto> postsList = new List<PostDto>();
            try
            {

                using (var con = new SqlConnection(Db.GetConnectionString()))
                {
                    await con.OpenAsync();

                    string sql = "SELECT * FROM POST ORDER BY LIKES,COMMENTS";
                    using (var cmd = new SqlCommand(sql, con))
                    {
                       
                        //var sr = await cmd.ExecuteReaderAsync();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                PostDto comment = new PostDto
                                {
                                    UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
                                    PostId = reader.GetInt32(reader.GetOrdinal("post_id")),
                                    Likes = reader.GetInt32(reader.GetOrdinal("likes")),
                                    Comments = reader.GetInt32(reader.GetOrdinal("comments")),
                                    PostData = reader.GetString(reader.GetOrdinal("content")),
                                    PostCreatedAt = reader.GetDateTime(reader.GetOrdinal("createdAt"))
                                };

                                postsList.Add(comment);
                            }
                            return postsList;
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private string SavePostFile(string userId,string userName)
		{
			string path = @"/Users/75way/Documents/Posts";
            string path2 = Path.Combine(path, $"{userId}-{userName}");
            //DirectoryInfo di = new DirectoryInfo(path);
            if (!Directory.Exists(path2)){
                Directory.CreateDirectory(path+"/"+userId+"-"+userName);
            }
            DateTime today = DateTime.Now;
            string fileName = $"{today:yyyyMMdd_HHmmss}.txt";
            string filePath = Path.Combine(path2, fileName);
            File.WriteAllText( filePath,"Hi My Name is sumit ");

            return filePath;
        }

	}
}

