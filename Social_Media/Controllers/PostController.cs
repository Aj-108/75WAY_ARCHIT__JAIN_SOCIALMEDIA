using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social_Media.Dto;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Social_Media.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]



    public class PostController : ControllerBase
    {

        public IConfiguration Config { get; set; }
        public IPostService Post_service { get; set; }

        public PostController(IConfiguration _config, IPostService _postservice)
        {
            Config = _config;
            Post_service = _postservice;
        }

        [HttpPost("createPost/{id}")]
        public async Task<IActionResult> CreatePost([FromBody] string postData,string id)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.UserData).ToString();
                if (id != userId) return BadRequest(new { status = false, message = "Unauthorized for this operation " });

                

                string userName = User.FindFirstValue(ClaimTypes.Name).ToString();
                PostDto post = new()
                {
                    UserId = Int32.Parse(userId),
                    PostData = postData,
                    PostCreatedAt = DateTime.Now 
                };
                ResponseDto result = await Post_service.CreatePost(post,id,userName);
                return Ok(new {status=result.Status,message=result.Message});

            }catch(Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message});
            }
        }


        [HttpGet("likePost/{postId}")]
        public async Task<IActionResult> LikePost(string postId)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.UserData).ToString();
                ResponseDto result = await Post_service.LikePost(Int32.Parse(postId), Int32.Parse(userId));
                if (result.Status)
                    return Ok(new { status = result.Status, message = result.Message });
                else
                    return BadRequest(new { status = false, message = result.Message });
            }
            catch(Exception Ex)
            {
                return BadRequest(new { status = false, message = Ex.Message });
            }
        }


        [HttpPost("commentPost/{postId}")]
        public async Task<IActionResult> LikePost([FromBody] string comment,string postId)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.UserData).ToString();
                ResponseDto result = await Post_service.CommentPost(Int32.Parse(postId), Int32.Parse(userId),comment);
                if (result.Status)
                    return Ok(new { status = result.Status, message = result.Message });
                else
                    return BadRequest(new { status = false, message = result.Message });
            }
            catch (Exception Ex)
            {
                return BadRequest(new { status = false, message = Ex.Message });
            }
        }

        [HttpGet("getComments/{postId}")]
        public async Task<IActionResult> GetComments(string postId)
        {
            try
            {
                List<CommentDto> result = await Post_service.GetComments(Int32.Parse(postId));
                    return Ok(new { status = true, comments = result });
                
            }
            catch (Exception Ex)
            {
                return BadRequest(new { status = false, message = Ex.Message });
            }
        }

        [HttpGet("getPosts")]
        public async Task<IActionResult> GetPosts()
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.UserData).ToString();
                List<PostDto> result = await Post_service.GetPosts(Int32.Parse(userId));
                return Ok(new { status = true, posts = result });

            }
            catch (Exception Ex)
            {
                return BadRequest(new { status = false, message = Ex.Message });
            }
        }

        [HttpGet("getTopPosts")]
        public async Task<IActionResult> GetTopPosts()
        {
            try
            {
                List<PostDto> result = await Post_service.GetTopPosts();
                return Ok(new { status = true, posts = result });

            }
            catch (Exception Ex)
            {
                return BadRequest(new { status = false, message = Ex.Message });
            }
        }
    }
}

