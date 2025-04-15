
using System.Threading.Tasks;
using global::Internal_API.controller;
using global::Internal_API.model;
using global::Internal_API.service;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Internal_API.Tests
{
    [TestClass]
    public class AuthControllerTests
    {
        private Mock<IUserInfoDao> mockUserInfoDao;
        private Mock<IJwtTokenService> mockJwtTokenService;
        private AuthController authController;

        [TestInitialize]
        public void Setup()
        {
            mockUserInfoDao = new Mock<IUserInfoDao>();
            mockJwtTokenService = new Mock<IJwtTokenService>();
            authController = new AuthController(mockUserInfoDao.Object, mockJwtTokenService.Object);
        }

        [TestMethod]
        public async Task Register_UserAlreadyExists_ReturnsBadRequest()
        {
            var user = new UserInfo { username = "testuser", passwordhash = "password" };
            mockUserInfoDao.Setup(x => x.GetUserByUsernameAsync(user.username)).ReturnsAsync(user);

            var result = await authController.Register(user);

            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Register_NewUser_ReturnsOk()
        {
            var user = new UserInfo { username = "newuser", passwordhash = "password" };
            mockUserInfoDao.Setup(x => x.GetUserByUsernameAsync(user.username)).ReturnsAsync((UserInfo)null);
            mockUserInfoDao.Setup(x => x.CreateUserAsync(It.IsAny<UserInfo>())).Returns(Task.CompletedTask);

            var result = await authController.Register(user);

            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            var login = new Login { username = "test", userrole = "user" };
            var user = new UserInfo { username = "test", userrole = "user" };

            mockUserInfoDao.Setup(x => x.VerifyUserPassword(login)).ReturnsAsync(true);
            mockUserInfoDao.Setup(x => x.GetUserByUsernameAsync(login.username)).ReturnsAsync(user);
            mockJwtTokenService.Setup(x => x.GenerateToken(user)).Returns("mock-token");

            var result = await authController.Login(login);

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var tokenProperty = okResult.Value.GetType().GetProperty("Token");
            var tokenValue = tokenProperty?.GetValue(okResult.Value)?.ToString();

            Assert.AreEqual("mock-token", tokenValue);
        }
        [TestMethod]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            var login = new Login { username = "invaliduser", password = "wrongpassword" };
            mockUserInfoDao.Setup(x => x.VerifyUserPassword(login)).ReturnsAsync(false);

            var result = await authController.Login(login);

            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
        }
    }
}


