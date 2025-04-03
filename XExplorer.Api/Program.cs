using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// 配置 Kestrel 端口
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(55000); // HTTP 端口
    // serverOptions.ListenAnyIP(5001, listenOptions => listenOptions.UseHttps()); // HTTPS 端口
});

// 配置 Serilog 日志
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(
        path: "logs/XExplorer.api.log",  // 日志文件路径
        rollingInterval: RollingInterval.Day,  // 按天分割日志
        retainedFileCountLimit: 30,  // 保留最近30天日志
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

// 添加日志服务
builder.Host.UseSerilog();
var app = builder.Build();
app.MapScalarApiReference(); // 映射到 /scalar/v1 

// app.UseHttpsRedirection();
// app.UseAuthorization(); 
app.MapControllers();
app.Run();