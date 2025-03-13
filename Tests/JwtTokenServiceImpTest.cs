using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Internal_API.service.Implementation;
using Internal_API.model;

namespace Internal_API.Tests
{
    [TestClass]
    public class JwtTokenServiceImpTests
    {
        private JwtTokenServiceImp jwtTokenService;
        private Mock<IConfiguration> mockConfiguration;

        [TestInitialize]
        public void Setup()
        {
            mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(config => config["Jwt:Key"]).Returns("ThisIsASecretKeyForTestingOnly!ChangeMe!");
            mockConfiguration.Setup(config => config["Jwt:Issuer"]).Returns("testIssuer");
            mockConfiguration.Setup(config => config["Jwt:Audience"]).Returns("testAudience");

            jwtTokenService = new JwtTokenServiceImp(mockConfiguration.Object);
        }

        [TestMethod]
        public void GenerateToken_ValidUser_ReturnsToken()
        {
            // Arrange
            var user = new UserInfo
            {
                username = "testuser",
                email = "testuser@example.com"
            };

            // Act
            var token = jwtTokenService.GenerateToken(user);

            // Assert
            Assert.IsNotNull(token);
            Assert.IsInstanceOfType(token, typeof(string));
            Assert.IsTrue(token.Length > 0);
        }
    }
}
