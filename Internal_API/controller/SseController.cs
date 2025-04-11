using Azure;
using Internal_API.service;
using Internal_API.service.Implementation;
using Microsoft.AspNetCore.Mvc;

namespace Internal_API.controller
{
    [ApiController]
    [Route("api/sse")]
    public class SseController : ControllerBase
    {
        private readonly IMessagePushService pushService;

        public SseController(IMessagePushService pushService)
        {
            this.pushService = pushService;
        }

        [HttpGet]
        public async Task Get()
        {
            var client = new SseClientImpl(Response);
            pushService.Register(client);

            try
            {
                await client.ListenAsync(HttpContext.RequestAborted);
            }
            finally
            {
                pushService.Unregister(client);
            }
        }
    }

}
