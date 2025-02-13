using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Testing.Platform.Configurations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Internal_API.Services;
using Internal_API.Services.Implementation;
using Internal_API.models;
using Amazon.S3.Transfer;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using Microsoft.AspNetCore.Http;

[TestClass]
public class S3ServiceImpTests
{
    private Mock<IConfiguration> _mockConfig;
    private Mock<IAmazonS3> _mockS3Client;
    private Mock<IFormFile> _mockFile;
    private IS3Service _s3Service;
    [TestInitialize] // Runs before each test
    public void Setup()
    {
        // Mock Configuration
        _mockConfig = new Mock<IConfiguration>();
        _mockConfig.Setup(c => c["AWS:BucketName"]).Returns("test-bucket");
        _mockConfig.Setup(c => c["AWS:Region"]).Returns("us-east-1");
        _mockFile = new Mock<IFormFile>();
        _mockS3Client = new Mock<IAmazonS3>();

        // Inject Mocked Dependencies
        _s3Service = new S3ServiceImp(_mockConfig.Object, _mockS3Client.Object);
    }

    [TestMethod]
    public async Task UploadFileAsync_ShouldReturnEmptyBucketMessage_WhenBucketNameIsNull()
    {
        // Arrange: Modify config to return null bucket
        _mockConfig.Setup(c => c["AWS:BucketName"]).Returns((string)null);
        IS3Service s3Service = new S3ServiceImp(_mockConfig.Object, null);

        var fileInfo = new S3FileInfo
        {
            file = _mockFile.Object,
            fileName = "test.jpg",
            type = "image/jpeg",
            userInfo = "user123",
            year = 2024
        };

        // Act
        var result = await s3Service.UploadFileAsync(fileInfo);

        // Assert
        Assert.AreEqual("empty bucket", result);
    }

    [TestMethod]
    public async Task UploadFileAsync_ShouldThrowException_WhenFileIsNull()
    {
        // Arrange
        var fileInfo = new S3FileInfo
        {
            file = null,
            fileName = "waybill.txt",
            type = "txt",
            userInfo = "user123",
            year = 2024
        };
        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
            await _s3Service.UploadFileAsync(fileInfo));
    }

    [TestMethod]
    public async Task UploadFileAsync_ShouldUploadFileSuccessfully()
    {
        // Arrange
        var fileContent = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
        _mockFile.Setup(f => f.Length).Returns(fileContent.Length);
        _mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default)).Returns((Stream stream, System.Threading.CancellationToken _) => fileContent.CopyToAsync(stream));
        IS3Service s3Service = new S3ServiceImp(_mockConfig.Object, _mockS3Client.Object);

        var fileInfo = new S3FileInfo
        {
            file = _mockFile.Object,
            fileName = "testfile.txt",
            type = "text/plain",
            year = 2024
        };

        // Act
        var result = await s3Service.UploadFileAsync(fileInfo);

        // Assert
        Assert.IsTrue(result.Contains("File uploaded successfully"));
    }

    [TestMethod]
    public async Task UploadFileAsync_ShouldReturnFailureMessage_WhenExceptionOccurs()
    {
        // Arrange
        var fileContent = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 });
        _mockFile.Setup(f => f.Length).Returns(fileContent.Length);
        _mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default)).Returns((Stream stream, System.Threading.CancellationToken _) => fileContent.CopyToAsync(stream));
        _mockConfig.Setup(c => c["AWS:Region"]).Returns("invalid-region");
        
        _mockS3Client.Setup(s3 => s3.PutObjectAsync(It.IsAny<Amazon.S3.Model.PutObjectRequest>(), It.IsAny<CancellationToken>()))
     .ThrowsAsync(new AmazonS3Exception("Mocked S3 upload failure"));
        IS3Service s3Service = new S3ServiceImp(_mockConfig.Object, _mockS3Client.Object);

        var fileInfo = new S3FileInfo
        {
            file = _mockFile.Object,
            fileName = "testfile.txt",
            type = "text/plain",
            year = 2024
        };

        await Assert.ThrowsAsync<Exception>(() => s3Service.UploadFileAsync(fileInfo));
        // Act
        //var result = await s3Service.UploadFileAsync(fileInfo);

        //// Assert
        //Assert.AreEqual("file failed", result);
    }

}