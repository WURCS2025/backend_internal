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
        private readonly IUserInfoDao userInfoDao;
        private readonly IJwtTokenService _jwtService;

        public AuthController(IUserInfoDao userInfoDao, IJwtTokenService jwtService)
        {
            this.userInfoDao = userInfoDao;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserInfo user)
        {
            var existingUser = await userInfoDao.GetUserByUsernameAsync(user.username);
            if (existingUser != null)
                return BadRequest("Username already exists");

            user.passwordhash = PasswordHelper.HashPassword(user.passwordhash);
            await userInfoDao.CreateUserAsync(user);
            return Ok("User registered successfully");
        }

        [HttpPost("chgpwd")]
        public async Task<IActionResult> ChangePwd([FromBody] UpdateLogin login)
        {
            if (!await userInfoDao.VerifyUserPassword(login))
            {
                return Unauthorized("Invalid credentials");
            }
            else
            {
                if (login.newPwd.Equals(login.confirmPwd))
                {
                    var existingUser = await userInfoDao.GetUserByUsernameAsync(login.username);

                    existingUser.passwordhash = login.newPwd.HashPassword();

                    await userInfoDao.UpdateUserAsync(existingUser);

                    return Ok("Password updated successfully");
                }
                else
                {
                    return BadRequest("Passwords aren't the same");
                }               
                
                
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Login login)
        {
            if (await userInfoDao.VerifyUserPassword(login))
            {
                var user = await userInfoDao.GetUserByUsernameAsync(login.username);
                if (login.userrole.ToLower().Contains("admin") && !user.userrole.ToLower().Contains("admin"))
                {
                    return Unauthorized("No admin access");
                }
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

