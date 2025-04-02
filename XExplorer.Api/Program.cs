using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
app.MapScalarApiReference(); // 映射到 /scalar/v1 

// app.UseHttpsRedirection();
// app.UseAuthorization();

app.MapControllers();
app.Run();