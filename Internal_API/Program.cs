using Amazon.S3;
using Internal_API.dao;
using Internal_API.data;
using Internal_API.models;
using Internal_API.service;
using Internal_API.service.Implementation;
using Internal_API.Services;
using Internal_API.Services.Implementation;
using Microsoft.EntityFrameworkCore;
using Amazon.SimpleNotificationService;
using Amazon.SQS;
using Amazon;
using Internal_API.model;
using Microsoft.AspNetCore.Mvc;


var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.ConfigureKestrel(serverOptions =>
//{
//    serverOptions.ListenAnyIP(5200); // HTTP
//    serverOptions.ListenAnyIP(5201, listenOptions => listenOptions.UseHttps()); // HTTPS
//});

//// Force the application to use the specified ports
//builder.WebHost.UseUrls("http://+:8082", "https://+:8083");

// Add services to the container.

// Register DbContext with the connection string from appsettings.json
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddScoped<IS3Service, S3ServiceImp>();
builder.Services.AddScoped<IFileUploadDao, FileUploadDao>();
builder.Services.AddScoped<IUserInfoDao, UserInfoDao>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenServiceImp>();
builder.Services.AddAWSService<IAmazonSimpleNotificationService>();
builder.Services.AddSingleton<IAmazonSQS>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var awsOptions = AwsOptions.readFromEnv();

    var sqsConfig = new AmazonSQSConfig
    {
        RegionEndpoint = RegionEndpoint.GetBySystemName(awsOptions.Region)
    };


    return new AmazonSQSClient(awsOptions.AccessKeyId, awsOptions.SecretAccessKey, sqsConfig);
});
//builder.Services.AddAWSService<IAmazonSQS>();
builder.Services.AddSingleton<IMessagePushService, MessagePushServiceImpl>(); // Your SSE message broker
builder.Services.AddHostedService<SqsPollingService>(); // ✅ This starts the polling service
builder.Services.AddHttpClient("UnsafeClient")
    .ConfigurePrimaryHttpMessageHandler(() =>
        new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
            AllowAutoRedirect = true
        });


builder.Services.AddEndpointsApiExplorer();


// ? Step 1: Define the CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocal", p => p
        .WithOrigins("http://localhost:5173", "https://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()); // only if you actually use cookies
});

//builder.Services.AddControllers();

//builder.Services.AddControllers(options =>
//{
//    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
//});

builder.Services.AddControllers().ConfigureApiBehaviorOptions(o =>
{
    o.SuppressModelStateInvalidFilter = true; // <-- disable auto 400 for debugging

});

// ? Step 2: Enable CORS Before Mapping Controllers

var app = builder.Build();

app.UseCors("AllowAll");
app.UseCors("AllowLocal");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
        context.Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");
        context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
        context.Response.StatusCode = 200;
        await context.Response.CompleteAsync();
    }
    else
    {
        await next();
    }
});

app.Run();
