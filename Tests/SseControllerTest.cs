using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Internal_API.controller;
using Internal_API.service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System;
using global::Internal_API.controller;
using global::Internal_API.service;

namespace Internal_API_Test
{


    namespace Internal_API.Tests.Controllers
    {
        [TestClass]
        public class SseControllerTests
        {
            private Mock<IMessagePushService> mockPushService;
            private SseController controller;

            [TestInitialize]
            public void Setup()
            {
                mockPushService = new Mock<IMessagePushService>();

                // Set up fake HttpContext with a dummy Response object
                var context = new DefaultHttpContext();
                context.Response.Body = new MemoryStream(); // Needed so Response.WriteAsync doesn't throw
                context.RequestAborted = new CancellationTokenSource().Token;

                controller = new SseController(mockPushService.Object)
                {
                    ControllerContext = new ControllerContext
                    {
                        HttpContext = context
                    }
                };
            }

            [TestMethod]
            public async Task Get_ShouldRegisterAndUnregisterClient()
            {
                // Arrange
                var registered = false;
                var unregistered = false;

                // Override IMessagePushService.Register to capture the client
                SseClientImpl capturedClient = null;
                mockPushService.Setup(s => s.Register(It.IsAny<SseClientImpl>()))
                               .Callback<SseClientImpl>(client =>
                               {
                                   registered = true;
                                   capturedClient = client;
                               });

                mockPushService.Setup(s => s.Unregister(It.IsAny<SseClientImpl>()))
                               .Callback(() => unregistered = true);

                // Override ListenAsync to simulate short-lived connection
                var listenTask = Task.CompletedTask;
                var clientImplMock = new Mock<SseClientImpl>(controller.Response) { CallBase = true };
                clientImplMock.Setup(c => c.ListenAsync(It.IsAny<CancellationToken>())).Returns(listenTask);

                // Act
                await controller.Get();

                // Assert
                Assert.IsTrue(registered);
                Assert.IsTrue(unregistered);
            }
        }
    }

}
