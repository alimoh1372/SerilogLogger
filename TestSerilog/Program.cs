using SerilogLogger;
using SerilogLogger.Abstraction.Dtos;
using SerilogLogger.Implementation;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddLoggerDependencies(new ApplicationLogConfiguration
{
    ApplicationId = "700",
    ApplicationName = "SerilogTest",
    IsLogToQueue = false
});

var app = builder.Build();


app.UseSwagger();

app.UseSwaggerUI();

app.MapControllers();

app.Run();
