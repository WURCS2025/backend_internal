
using Moq;
using MockQueryable;
using Internal_API.dao;
using Internal_API.model;
using Internal_API.utility;
using Microsoft.EntityFrameworkCore;

using Internal_API.models;
using MockQueryable.Moq;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;


namespace Internal_API.Tests
{
   
    [TestClass]
    public class UserInfoDaoTests
    {
        private Mock<AppDbContext> mockDbContext;
        private UserInfoDao userInfoDao;
        private Mock<DbSet<UserInfo>> mockDbSet;
        private List<UserInfo> userInfoList;
        private string passwordNotHashed = "hashedpassword3";

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptions<AppDbContext>();
            mockDbContext = new Mock<AppDbContext>(options);
            var storedhash = passwordNotHashed.HashPassword();
            userInfoList = new List<UserInfo>
        {
            new UserInfo { Id = new Guid(), Username = "testuser1", PasswordHash = "hashedpassword1" },
            new UserInfo { Id = new Guid(), Username = "testuser2", PasswordHash = "hashedpassword2" },
            new UserInfo { Id = new Guid(), Username = "testuser3", PasswordHash = storedhash }
        };

            mockDbSet = userInfoList.AsQueryable().BuildMockDbSet();

            //mockDbSet = new Mock<DbSet<UserInfo>>();
            //mockDbSet.As<IQueryable<UserInfo>>().Setup(m => m.Provider).Returns(userInfoList.AsQueryable().Provider);
            //mockDbSet.As<IQueryable<UserInfo>>().Setup(m => m.Expression).Returns(userInfoList.AsQueryable().Expression);
            //mockDbSet.As<IQueryable<UserInfo>>().Setup(m => m.ElementType).Returns(userInfoList.AsQueryable().ElementType);
            //mockDbSet.As<IQueryable<UserInfo>>().Setup(m => m.GetEnumerator()).Returns(userInfoList.GetEnumerator());

            mockDbContext.Setup(db => db.UserInfo).Returns(mockDbSet.Object);

            userInfoDao = new UserInfoDao(mockDbContext.Object);

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
            mockDbContext.Verify(db => db.SaveChangesAsync(default), Times.Once);
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
            mockDbContext.Verify(db => db.SaveChangesAsync(default), Times.Once);
        }

        [TestMethod]
        public async Task GetUserByUsernameAsync_ExistingUser_ReturnsUser()
        {

    //        var options = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(databaseName: "TestDb")  // In-memory database
    //.Options;

    //        AppDbContext _context = new AppDbContext(options);
    //        UserInfoDao _userInfoDao = new UserInfoDao(_context);

    //        var testUser = new UserInfo { Id = new Guid(), Username = "testuser1", PasswordHash = "hashedpassword" };
    //        await _context.UserInfo.AddAsync(testUser);
    //        await _context.SaveChangesAsync();

            // Act
            var result = await userInfoDao.GetUserByUsernameAsync("testuser1");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("testuser1", result.Username);
        }

        [TestMethod]
        public async Task VerifyUserPassword_CorrectPassword_ReturnsTrue()
        {           

            UserInfo user = new UserInfo { Username = "testuser3", PasswordHash = passwordNotHashed };
            // Act
            var result = await userInfoDao.VerifyUserPassword(user);

            // Assert
            Assert.IsTrue(result);
        }
    }
}
