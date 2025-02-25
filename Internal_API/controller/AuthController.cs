using Internal_API.dao;
using Internal_API.model;
using Internal_API.models;
using Internal_API.service;
using Internal_API.service.Implementation;
using Internal_API.utility;
using Microsoft.AspNetCore.Mvc;

namespace Internal_API.controller
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserInfoDao userInfoDao;
        private readonly IJwtTokenService _jwtService;

        public AuthController(UserInfoDao userInfoDao, IJwtTokenService jwtService)
        {
            this.userInfoDao = userInfoDao;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserInfo user)
        {
            var existingUser = await userInfoDao.GetUserByUsernameAsync(user.Username);
            if (existingUser != null)
                return BadRequest("Username already exists");

            user.PasswordHash = PasswordHelper.HashPassword(user.PasswordHash);
            await userInfoDao.CreateUserAsync(user);
            return Ok("User registered successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserInfo user)
        {
            if (await userInfoDao.VerifyUserPassword(user))
            {
                var token = _jwtService.GenerateToken(user);
                return Ok(new { Token = token });
            }
            else
            {
                return Unauthorized("Invalid credentials");
            }
            
        }
    }
}

