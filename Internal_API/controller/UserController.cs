using Internal_API.constants;
using Internal_API.dao;
using Internal_API.model;
using Internal_API.models;
using Internal_API.service;
using Internal_API.service.Implementation;
using Internal_API.utility;
using Microsoft.AspNetCore.Mvc;

namespace Internal_API.controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserInfoDao userInfoDao;
        private readonly IJwtTokenService _jwtService;

        public UserController(IUserInfoDao userInfoDao, IJwtTokenService jwtService)
        {
            this.userInfoDao = userInfoDao;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserView userview)
        {
            var existingUser = await userInfoDao.GetUserByUsernameAsync(userview.username);
            if (existingUser != null)
                return BadRequest("Username already exists");

            UserInfo user = new UserInfo(userview);

            user.passwordhash = PasswordHelper.HashPassword(user.passwordhash);
            await userInfoDao.CreateUserAsync(user);
            return Ok(user);
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

        [HttpGet("userlist")]
        public async Task<IActionResult> getUserList([FromQuery] string username)
        {
            var user = await userInfoDao.GetUserByUsernameAsync(username);

            if (user.userrole.ToLower() == UserRoles.USER)
            {
                return BadRequest("Only admin or analysts can see user list");
            }

            var userList = await userInfoDao.GetUserList();
            var userViewList = userList.Select(a => new UserView(a)).ToList();
            
            var usernameList = userList.Select(a => a.username).ToList();
            return Ok(userViewList);
        }

        [HttpPost("deleteUser")]
        public async Task<IActionResult> deleteUser([FromBody] DeleteUserRequest request)
        {
            var user = await userInfoDao.GetUserByUsernameAsync(request.admin);

            if (user.userrole.ToLower() != UserRoles.ADMIN)
            {
                return BadRequest("Only admin can delete an user");
            }

            try
            {
                userInfoDao.DeleteUser(request.username);
                return Ok(request.username);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            
        }
    }
}

