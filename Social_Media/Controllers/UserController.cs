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
    public class UserController : ControllerBase
    {

        public IConfiguration Config { get; set; }
        public IUserService User_service { get; set; }

        public UserController(IConfiguration _config,IUserService _userService)
        {
            Config = _config;
            User_service = _userService;
        }

       [AllowAnonymous]
       [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserRegisterDto userData)
        {
            try
            {
                ResponseDto result = await User_service.UserRegister(userData);

                return Ok(new {status = result.Status , message = result.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> LoginUser([FromBody] UserLoginDto userData)
        {
            try
            {
                ResponseDto result = await User_service.UserLogin(userData);

                return Ok(new { status = result.Status, message = result.Message , token = result.Data });
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = false, message = ex.Message });
            }
        }

        [HttpGet("follow/{userId}")]
        public async Task<IActionResult> LikePost(string userId)
        {
            try
            {
                string followingUserId = User.FindFirstValue(ClaimTypes.UserData).ToString();
                ResponseDto result = await User_service.FollowUser(Int32.Parse(userId), Int32.Parse(followingUserId));
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


        [HttpGet("getTopFollow/")]
        public async Task<IActionResult> getTopFolow()
        {
            try
            {
                List<string> result = await User_service.getTopFollow();
                
                    return Ok(new { status = true, message = result });
               
            }
            catch (Exception Ex)
            {
                return BadRequest(new { status = false, message = Ex.Message });
            }
        }
    }
}

