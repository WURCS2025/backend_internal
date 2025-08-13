using Microsoft.AspNetCore.Mvc;
using Internal_API.Services;
using Internal_API.models;
using Internal_API.Services.Implementation;
using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Internal_API.model;
using Internal_API.constants;
using Internal_API.service;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using System.Text.Json;

namespace Internal_API.controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController : ControllerBase
    {
        private readonly IAmazonSimpleNotificationService snsClient;
        private readonly IConfiguration configuration;
        private readonly IFileUploadDao fileUploadDao;
        private readonly string snsTopicArn = "arn:aws:sns:us-east-1:123456789012:YourTopic";

        public MessageController(IAmazonSimpleNotificationService snsClient, IConfiguration configuration, IFileUploadDao fileUploadDao)
        {
            this.snsClient = snsClient;
            this.configuration = configuration;
            snsTopicArn = configuration["AWS:PushDataTopicArn"];
            this.fileUploadDao = fileUploadDao;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] string fileid)
        {
            var fileupload = fileUploadDao.GetUploadById(fileid);


            if (fileupload != null)
            {
                string jsonMessage = JsonSerializer.Serialize(fileupload);
                var publishRequest = new PublishRequest
                {
                    TopicArn = snsTopicArn,
                    Message = jsonMessage
                };

                await snsClient.PublishAsync(publishRequest);

                fileupload.status = FileStatus.processing.ToString();
                await fileUploadDao.saveChanges();
                return Ok(fileupload);
            }
            else
            {
                return BadRequest("File id invalid");
            }
        }
    }

}
