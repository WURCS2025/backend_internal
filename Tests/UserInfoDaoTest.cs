using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Internal_API.dao;
using Internal_API.model;
using Internal_API.utility;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

using Internal_API.models;
using Internal_API.utility;
using Internal_API.data;

namespace Internal_API.Tests
{
    [TestClass]
    public class UserInfoDaoTests
    {
        private Mock<AppDbContext> mockDbContext;
        private UserInfoDao userInfoDao;
        private Mock<AppDbContext> mockContext;
        private Mock<DbSet<UserInfo>> mockDbSet;
        private List<UserInfo> userInfoList;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptions<AppDbContext>();
            mockDbContext = new Mock<AppDbContext>(options);
            userInfoDao = new UserInfoDao(mockDbContext.Object);
            userInfoList = new List<UserInfo>
        {
            new UserInfo { Id = new Guid(), Username = "testuser1", PasswordHash = "hashedpassword1" },
            new UserInfo { Id = new Guid(), Username = "testuser2", PasswordHash = "hashedpassword2" }
        };

        }

        [TestMethod]
        public async Task CreateUserAsync_ValidUser_AddsUserToDatabase()
        {
            // Arrange
            var user = new UserInfo { Id = new Guid(), Username = "testuser3", PasswordHash = "hashedpassword3" };

            // Act
            await userInfoDao.CreateUserAsync(user);

            // Assert
            mockDbSet.Verify(db => db.AddAsync(user, default), Times.Once);
            mockContext.Verify(db => db.SaveChangesAsync(default), Times.Once);
        }

        [TestMethod]
        public async Task DeleteUserAsync_ExistingUser_RemovesUserFromDatabase()
        {
            // Arrange
            var user = new UserInfo { Id = new Guid(), Username = "testuser" };
            var users = new List<UserInfo> { user }.AsQueryable();
            mockDbSet.As<IQueryable<UserInfo>>().Setup(m => m.Provider).Returns(users.Provider);
            mockDbSet.As<IQueryable<UserInfo>>().Setup(m => m.Expression).Returns(users.Expression);
            mockDbSet.As<IQueryable<UserInfo>>().Setup(m => m.ElementType).Returns(users.ElementType);
            mockDbSet.As<IQueryable<UserInfo>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            // Act
            await userInfoDao.DeleteUserAsync(user);

            // Assert
            mockDbSet.Verify(db => db.Remove(user), Times.Once);
            mockContext.Verify(db => db.SaveChangesAsync(default), Times.Once);
        }

        [TestMethod]
        public async Task GetUserByUsernameAsync_ExistingUser_ReturnsUser()
        {
            // Arrange
            var user = new UserInfo { Id = new Guid(), Username = "testuser" };
            var users = new List<UserInfo> { user }.AsQueryable();
            mockDbSet.As<IQueryable<UserInfo>>().Setup(m => m.Provider).Returns(users.Provider);
            mockDbSet.As<IQueryable<UserInfo>>().Setup(m => m.Expression).Returns(users.Expression);
            mockDbSet.As<IQueryable<UserInfo>>().Setup(m => m.ElementType).Returns(users.ElementType);
            mockDbSet.As<IQueryable<UserInfo>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            // Act
            var result = await userInfoDao.GetUserByUsernameAsync("testuser");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("testuser", result.Username);
        }

        [TestMethod]
        public async Task VerifyUserPassword_CorrectPassword_ReturnsTrue()
        {
            // Arrange
            var user = new UserInfo { Username = "testuser", PasswordHash = "hashedpassword" };
            var storedhash = user.PasswordHash.HashPassword();
            var existinguser = new UserInfo { Username = "testuser", PasswordHash = storedhash };
            mockDbSet.Setup(m => m.FindAsync("testuser")).ReturnsAsync(existinguser);            

            // Act
            var result = await userInfoDao.VerifyUserPassword(user);

            // Assert
            Assert.IsTrue(result);
        }
    }
}
