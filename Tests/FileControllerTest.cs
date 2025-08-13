
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Internal_API.Controllers;
using Internal_API.models;
using Internal_API.Services;
using Internal_API.constants;
using Internal_API.dao;
using Internal_API.service;

namespace Internal_API.Tests
{
    [TestClass]
    public class FileControllerTest
    {
        private Mock<IFileUploadDao> _mockFileUploadDao;
        private Mock<IConfiguration> _mockConfiguration;
        private S3FileController _controller;
        private Mock<IUserInfoDao> _mockUserInfoDao;

        [TestInitialize]
        public void Setup()
        {
            // Mock AWS Configuration
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(c => c["AWS:Region"]).Returns("us-east-1");
            _mockConfiguration.Setup(c => c["AWS:AccessKey"]).Returns("fake-access-key");
            _mockConfiguration.Setup(c => c["AWS:SecretKey"]).Returns("fake-secret-key");

            // Mock File Upload DAO
            _mockFileUploadDao = new Mock<IFileUploadDao>();
            _mockUserInfoDao = new Mock<IUserInfoDao>();

            // Initialize Controller with Mocked Dependencies
            _controller = new S3FileController(_mockConfiguration.Object, _mockFileUploadDao.Object, _mockUserInfoDao.Object);
        }

        // ✅ TEST: UploadFile should return 200 OK on success
        [TestMethod]
        public async Task UploadFile_ReturnsOk_WhenUploadIsSuccessful()
        {
            // Arrange
            var mockFile = new Mock<Microsoft.AspNetCore.Http.IFormFile>();
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write("Dummy File Content");
            writer.Flush();
            stream.Position = 0;

            mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
            mockFile.Setup(f => f.FileName).Returns("testfile.txt");
            mockFile.Setup(f => f.Length).Returns(stream.Length);

            var fileInfo = new S3FileInfo
            {
                file = mockFile.Object,
                fileName = "testfile.txt"
            };

            // Mock the file upload service response
            _mockFileUploadDao.Setup(f => f.SaveFileUpload(It.IsAny<S3FileInfo>()));

            // Act
            var result = await _controller.UploadFile(fileInfo);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        // ✅ TEST: UploadFile should return 400 BadRequest on Exception
        [TestMethod]
        public async Task UploadFile_ReturnsBadRequest_OnException()
        {
            // Arrange
            var mockFile = new Mock<Microsoft.AspNetCore.Http.IFormFile>();
            var stream = new MemoryStream();
            mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
            mockFile.Setup(f => f.FileName).Returns("testfile.txt");
            mockFile.Setup(f => f.Length).Returns(stream.Length);

            var fileInfo = new S3FileInfo
            {
                file = mockFile.Object,
                fileName = "testfile.txt"
            };

            // Force the DAO to throw an exception
            _mockFileUploadDao.Setup(f => f.SaveFileUpload(It.IsAny<S3FileInfo>()))
                .Throws(new Exception("Database error"));

            // Act
            var result = await _controller.UploadFile(fileInfo);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }

        // ✅ TEST: GetStatus should return correct file upload details
        [TestMethod]
        public void GetStatus_ReturnsFileUpload_WhenExists()
        {
            // Arrange
            var testGuid = Guid.NewGuid().ToString();
            var testFileUpload = new FileUpload
            {
                id = testGuid,
                filename = "testfile.txt",
                s3_key = "s3/testfile.txt",
                status = FileStatus.uploaded.ToString()
            };

            _mockFileUploadDao.Setup(f => f.GetUploadById(testGuid)).Returns(testFileUpload);

            // Act
            var result = _controller.GetStatus(testGuid);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(testGuid, result.id);
            Assert.AreEqual("testfile.txt", result.filename);
            Assert.AreEqual("s3/testfile.txt", result.s3_key);
            Assert.AreEqual(FileStatus.uploaded.ToString(), result.status);
        }

        // ✅ TEST: GetStatus should return null when file is not found
        [TestMethod]
        public void GetStatus_ReturnsNull_WhenFileNotFound()
        {
            // Arrange
            var testGuid = Guid.NewGuid().ToString();
            _mockFileUploadDao.Setup(f => f.GetUploadById(testGuid)).Returns((FileUpload)null);

            // Act
            var result = _controller.GetStatus(testGuid);

            // Assert
            Assert.IsNull(result);
        }
    }
}
