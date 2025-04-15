using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Internal_API_Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Microsoft.Extensions.Configuration;
    using Amazon.SimpleNotificationService;
    using Amazon.SimpleNotificationService.Model;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Threading.Tasks;
    using global::Internal_API.constants;
    using global::Internal_API.controller;
    using global::Internal_API.models;
    using global::Internal_API.Services;

    namespace Internal_API.Tests.Controllers
    {
        [TestClass]
        public class MessageControllerTests
        {
            private Mock<IAmazonSimpleNotificationService> mockSnsClient;
            private Mock<IConfiguration> mockConfig;
            private Mock<IFileUploadDao> mockFileUploadDao;
            private MessageController controller;

            [TestInitialize]
            public void Setup()
            {
                mockSnsClient = new Mock<IAmazonSimpleNotificationService>();
                mockConfig = new Mock<IConfiguration>();
                mockFileUploadDao = new Mock<IFileUploadDao>();

                mockConfig.Setup(cfg => cfg["AWS:PushDataTopicArn"]).Returns("arn:aws:sns:us-east-1:123456789012:YourTopic");

                controller = new MessageController(mockSnsClient.Object, mockConfig.Object, mockFileUploadDao.Object);
            }

            [TestMethod]
            public async Task SendMessage_ValidFileId_PublishesMessageAndUpdatesStatus()
            {
                // Arrange
                var fileId = Guid.NewGuid();
                var fileUpload = new FileUpload
                {
                    id = fileId,
                    filename = "testfile.txt",
                    status = FileStatus.uploaded
                };

                mockFileUploadDao.Setup(dao => dao.GetUploadById(fileId)).Returns(fileUpload);
                mockSnsClient.Setup(sns => sns.PublishAsync(It.IsAny<PublishRequest>(), default))
                             .ReturnsAsync(new PublishResponse());

                // Act
                var result = await controller.SendMessage(fileId.ToString());

                // Assert
                Assert.IsInstanceOfType(result, typeof(OkObjectResult));
                var okResult = result as OkObjectResult;
                Assert.AreEqual(FileStatus.processing, ((FileUpload)okResult.Value).status);

                mockSnsClient.Verify(sns => sns.PublishAsync(It.IsAny<PublishRequest>(), default), Times.Once);
                mockFileUploadDao.Verify(dao => dao.saveChanges(), Times.Once);
            }

            [TestMethod]
            public async Task SendMessage_InvalidFileId_ReturnsBadRequest()
            {
                // Arrange
                var fileId = Guid.NewGuid();
                mockFileUploadDao.Setup(dao => dao.GetUploadById(fileId)).Returns((FileUpload)null);

                // Act
                var result = await controller.SendMessage(fileId.ToString());

                // Assert
                Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
                var badRequest = result as BadRequestObjectResult;
                Assert.AreEqual("File id invalid", badRequest.Value);
            }
        }
    }

}
