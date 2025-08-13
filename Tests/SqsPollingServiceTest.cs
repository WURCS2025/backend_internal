using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Internal_API.model;
using Internal_API.Services;
using Internal_API.service;
using Internal_API.models;
using Internal_API.constants;

namespace Internal_API_Test
{

    

    [TestClass]
    public class SqsPollingServiceTests
    {
        private Mock<IAmazonSQS> _mockSqs;
        private Mock<IMessagePushService> _mockPushService;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<ILogger<SqsPollingService>> _mockLogger;
        private Mock<IServiceScopeFactory> _mockScopeFactory;
        private Mock<IServiceScope> _mockServiceScope;
        private Mock<IServiceProvider> _mockServiceProvider;
        private Mock<IFileUploadDao> _mockFileUploadDao;

        private const string QueueUrl = "https://sqs.amazonaws.com/123/your-queue";

        [TestMethod]
        public async Task ExecuteAsync_ProcessesMessageCorrectly()
        {
            // Arrange
            var processingResult = new ProcessingResult
            {
                id = Guid.NewGuid().ToString(),
                message = "Processed successfully",
                process_result = "SUCCESS"
            };

            var messagePayload = new
            {
                Type = "Notification",
                Message = JsonSerializer.Serialize(processingResult)
            };

            var rawBody = JsonSerializer.Serialize(messagePayload);

            _mockSqs = new Mock<IAmazonSQS>();
            _mockPushService = new Mock<IMessagePushService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockLogger = new Mock<ILogger<SqsPollingService>>();
            _mockScopeFactory = new Mock<IServiceScopeFactory>();
            _mockServiceScope = new Mock<IServiceScope>();
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockFileUploadDao = new Mock<IFileUploadDao>();

            _mockConfiguration.Setup(c => c["AWS:OutputDataSQSUrl"]).Returns(QueueUrl);

            var fakeFile = new FileUpload
            {
                id = processingResult.id,
                status = FileStatus.processing.ToString(),
                message = ""
            };

            _mockFileUploadDao.Setup(d => d.GetUploadById(It.IsAny<string>()))
                              .Returns(fakeFile);

            _mockServiceProvider.Setup(sp => sp.GetService(typeof(IFileUploadDao)))
                                .Returns(_mockFileUploadDao.Object);

            _mockServiceScope.Setup(s => s.ServiceProvider).Returns(_mockServiceProvider.Object);
            _mockScopeFactory.Setup(f => f.CreateScope()).Returns(_mockServiceScope.Object);

            _mockSqs.Setup(s => s.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new ReceiveMessageResponse
                    {
                        Messages = new List<Message>
                        {
                        new Message
                        {
                            Body = rawBody,
                            ReceiptHandle = "fake-receipt-handle"
                        }
                        }
                    });

            _mockSqs.Setup(s => s.DeleteMessageAsync(QueueUrl, "fake-receipt-handle", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new DeleteMessageResponse());

            _mockFileUploadDao.Setup(d => d.saveChanges()).Returns(Task.CompletedTask);
            _mockPushService.Setup(p => p.PushAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

            var service = new SqsPollingService(
                _mockSqs.Object,
                _mockPushService.Object,
                _mockConfiguration.Object,
                _mockLogger.Object,
                _mockScopeFactory.Object);

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(3000); // Stop after 3 seconds

            // Act
            await service.StartAsync(cts.Token);
            await Task.Delay(1000); // Give it a chance to process
            await service.StopAsync(cts.Token);

            // Assert
            _mockPushService.Verify(p => p.PushAsync(It.Is<string>(s => s.Contains(processingResult.id))), Times.AtLeastOnce());
            _mockFileUploadDao.Verify(d => d.saveChanges(), Times.AtLeastOnce());
            _mockSqs.Verify(s => s.DeleteMessageAsync(QueueUrl, "fake-receipt-handle", It.IsAny<CancellationToken>()), Times.Once());
        }
    }

}
