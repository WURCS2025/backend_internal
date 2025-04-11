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
builder.Services.AddAWSService<IAmazonSQS>();
builder.Services.AddSingleton<IMessagePushService, MessagePushServiceImpl>(); // Your SSE message broker
builder.Services.AddHostedService<SqsPollingService>(); // ✅ This starts the polling service


// ? Step 1: Define the CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });

    
});

builder.Services.AddControllers();

// ? Step 2: Enable CORS Before Mapping Controllers

var app = builder.Build();

app.UseCors("AllowAll");

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
