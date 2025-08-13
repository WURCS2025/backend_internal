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
        _logger.LogInformation("Starting SQS Polling Service...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var request = new ReceiveMessageRequest
                {
                    QueueUrl = queueUrl,
                    MaxNumberOfMessages = 5,
                    WaitTimeSeconds = 20, // Long polling
                    VisibilityTimeout = 60
                };

                var response = await _sqsClient.ReceiveMessageAsync(request, stoppingToken);

                if (response.Messages.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken); // Wait a bit before polling again
                    continue;
                }

                foreach (var message in response.Messages)
                {
                    try
                    {
                        var rawSqsMessage = message.Body;
                        _logger.LogInformation($"Received SQS message: {rawSqsMessage}");

                        var outer = JsonDocument.Parse(rawSqsMessage);
                        var messageJson = outer.RootElement.GetProperty("Message").GetString();
                        var result = JsonSerializer.Deserialize<ProcessingResult>(messageJson);

                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var fileUploadDao = scope.ServiceProvider.GetRequiredService<IFileUploadDao>();
                            var fileupload = fileUploadDao.GetUploadById(result.id);

                            if (fileupload != null)
                            {
                                fileupload.status = result.process_result.ToUpper() switch
                                {
                                    "SUCCESS" => FileStatus.completed.ToString(),
                                    "FAILURE" => FileStatus.error.ToString(),
                                    _ => fileupload.status
                                };
                                fileupload.message = result.message;
                                await fileUploadDao.saveChanges();
                            }
                        }

                        var payload = new
                        {
                            id = result.id,
                            message = result.message,
                            result = result.process_result
                        };

                        var json = JsonSerializer.Serialize(payload);

                        await Task.Delay(3000, stoppingToken); // Replace Thread.Sleep

                        await pushService.PushAsync(json);
                        await _sqsClient.DeleteMessageAsync(queueUrl, message.ReceiptHandle, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing individual SQS message.");
                        // Optionally move to DLQ or log for re-processing
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Expected on shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SQS polling loop.");
                await Task.Delay(5000, stoppingToken); // Avoid hot loop on repeated failures
            }
        }

        _logger.LogInformation("SQS Polling Service stopped.");
    }


    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping SqsPollingService...");
        await base.StopAsync(cancellationToken);
    }
}



