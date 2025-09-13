using SerilogLogger.Abstraction.Dtos;
using SerilogLogger.Abstraction.LoggerInterface;
using SerilogLogger.Implementation;
using TestSerilog.Samples;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var appConfiguration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json",
        optional: true, reloadOnChange: true)
    .Build();

var applicationLogConfiguration = appConfiguration.GetSection(ApplicationLogConfiguration.ConfigurationSectionName)
    .Get<ApplicationLogConfiguration>()!;


builder.Services.AddLoggerDependencies(appConfiguration);

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.MapControllers();

var logger = app.Services.GetRequiredService<ILog>();
if (logger == null)
{
    throw new NotImplementedException("ILog");
}

var sample = new UsageSamples(logger);

sample.DemonstrateUsage();

app.Run();

