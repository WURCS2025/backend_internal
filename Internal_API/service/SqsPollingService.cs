using Amazon.SQS.Model;
using Amazon.SQS;
using Internal_API.service;
using System.Text.Json;
using Internal_API.service.Implementation;
using Internal_API.model;
using Internal_API.Services;
using Internal_API.data;
using Internal_API.models;

public class SqsPollingService : BackgroundService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IMessagePushService pushService;
    private readonly IConfiguration configuration;
    private readonly ILogger<SqsPollingService> _logger;
    private readonly string queueUrl;
    public SqsPollingService(IAmazonSQS sqsClient, IMessagePushService pushService, IConfiguration configuration, ILogger<SqsPollingService> logger)
    {
        _sqsClient = sqsClient;
        this.pushService = pushService;
        queueUrl = configuration["AWS:OutputDataSQSUrl"];
        this.configuration = configuration;
        _logger = logger;
    }


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Polling SQS...");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var request = new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl,
                    MaxNumberOfMessages = 5,
                    WaitTimeSeconds = 15
                };

                var response = await _sqsClient.ReceiveMessageAsync(request, stoppingToken);

                foreach (var message in response.Messages)
                {
                    var rawSqsMessage = message.Body;

                    _logger.LogInformation($"Received SQS message: {rawSqsMessage}");

                    var outer = JsonDocument.Parse(rawSqsMessage); // rawSqsMessage = msg.Body
                    var messageJson = outer.RootElement.GetProperty("Message").GetString();
                    ProcessingResult result = JsonSerializer.Deserialize<ProcessingResult>(messageJson);

                    // 👈 Your logic to get file ID from the SQS message
                    var payload = new
                    {
                        id = result.id,

                        message = result.message,
                        result = result.process_result
                    };

                    //var fileupload = fileUploadDao.GetUploadById(new Guid(result.id));

                    //if (result.process_result.ToUpper().Contains("SUCCESS"))
                    //{
                    //    fileupload.status = Internal_API.constants.FileStatus.completed;
                    //}
                    //else
                    //{
                    //    fileupload.status = Internal_API.constants.FileStatus.error;
                    //}
                    //await fileUploadDao.saveChanges();

                    var json = JsonSerializer.Serialize(payload);

                    System.Threading.Thread.Sleep(15000);

                    await pushService.PushAsync(json); // 👈 Push structured message
                    await _sqsClient.DeleteMessageAsync(queueUrl, message.ReceiptHandle);
                }
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error while polling SQS.");
        }
    }
}



