using Amazon.SQS.Model;
using Amazon.SQS;
using Internal_API.service;
using System.Text.Json;
using Internal_API.service.Implementation;
using Internal_API.model;
using Internal_API.Services;
using Internal_API.data;
using Internal_API.models;
using Internal_API.constants;

public class SqsPollingService : BackgroundService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IMessagePushService pushService;
    private readonly IConfiguration configuration;
    private readonly ILogger<SqsPollingService> _logger;
    private readonly string queueUrl;
    IServiceScopeFactory _scopeFactory;
    public SqsPollingService(IAmazonSQS sqsClient, IMessagePushService pushService, IConfiguration configuration, ILogger<SqsPollingService> logger, IServiceScopeFactory scopeFactory)
    {
        _sqsClient = sqsClient;
        this.pushService = pushService;
        queueUrl = configuration["AWS:OutputDataSQSUrl"];
        this.configuration = configuration;
        _logger = logger;
        this._scopeFactory = scopeFactory;
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

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var fileUploadDao = scope.ServiceProvider.GetRequiredService<IFileUploadDao>();

                        var fileupload = fileUploadDao.GetUploadById(new Guid(result.id));
                        fileupload.status = result.process_result.ToUpper() switch
                        {
                            "SUCCESS" => Internal_API.constants.FileStatus.completed,
                            "FAILURE" => Internal_API.constants.FileStatus.error,
                            _ => fileupload.status
                        };

                        fileupload.message = result.message;

                        await fileUploadDao.saveChanges();
                    }

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

                    System.Threading.Thread.Sleep(3000);

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

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping SqsPollingService...");
        await base.StopAsync(cancellationToken);
    }
}



