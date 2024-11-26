using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SerilogLogger.Abstraction.LoggerInterface;
using SerilogLogger.Utilities.Utilities;


namespace TestSerilog.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SerilogLoggerController : ControllerBase
    {
        private readonly ILog _logger;

        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private MyClass _myClass = new MyClass
        {
            endPoint = new IPEndPoint(IPAddress.Parse("192.168.10.10"), 2045),
            ArrayList = new MyClass[]
            {
                new MyClass
                {
                    endPoint = new IPEndPoint(IPAddress.Parse("192.168.10.20"), 4125),
                    ArrayList = new MyClass[]
                    {
                    },
                    JsonSerialized = JsonSerializer.Serialize(Summaries)
                }
            },
            JsonSerialized = JsonSerializer.Serialize(Summaries)
        };
        public SerilogLoggerController(ILog logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetLogTypesTest")]
        public IActionResult Get()
        {
            _logger.Debug("Test Log Information", Summaries.GetKeyValuePairs("Summaries"));

            _logger.Information("Test Log information", Summaries.GetKeyValuePairs("Summaries"));

            _logger.Warning("Test Log information", Summaries.GetKeyValuePairs("Summaries"));

            _logger.Error("Test Log information", Summaries.GetKeyValuePairs("Summaries"));

            _logger.Warning("Test class logging", _myClass.GetKeyValuePairs("MyClass"));

            return Ok($"Completed.");
        }

        [HttpPost(Name = "GetLogTime")]
        public string GetLogTime(int count)
        {

            var stopWatch = new Stopwatch();

            stopWatch.Start();


            Enumerable.Range(1, count).AsParallel()
                .WithDegreeOfParallelism(20)
                .ForAll((i) =>
                {
                    _logger.Information($"{i}-Test logger speed."

                        , new List<KeyValuePair<string, object>>
                        {
                            new KeyValuePair<string, object>(Guid.NewGuid().ToString().Substring(0, 5) + "- 1", Guid.NewGuid().ToString().Substring(0, 5)),
                            new KeyValuePair<string, object>(Guid.NewGuid().ToString().Substring(0, 5) + "- 2", Guid.NewGuid().ToString().Substring(0, 5)),
                            new KeyValuePair<string, object>(Guid.NewGuid().ToString().Substring(0, 5) + "- 3", Guid.NewGuid().ToString().Substring(0, 5)),
                            new KeyValuePair<string, object>(Guid.NewGuid().ToString().Substring(0, 5) + "- 4", Guid.NewGuid().ToString().Substring(0, 5)),
                            new KeyValuePair<string, object>(Guid.NewGuid().ToString().Substring(0, 5) + "- 5", Guid.NewGuid().ToString().Substring(0, 5))
                        }
                    );
                });

            stopWatch.Stop();

            var time = stopWatch.Elapsed.TotalMilliseconds;

            return time.ToString();
        }
    }

    public class MyClass
    {
        public IPEndPoint endPoint { get; set; }

        public MyClass[] ArrayList { get; set; }

        public string JsonSerialized { get; set; }

    }
}