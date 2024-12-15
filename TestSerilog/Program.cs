using SerilogLogger.Abstraction.Dtos;
using SerilogLogger.Implementation;

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


builder.Services.AddLoggerDependencies(applicationLogConfiguration);

var app = builder.Build();


app.UseSwagger();

app.UseSwaggerUI();

app.MapControllers();

app.Run();
