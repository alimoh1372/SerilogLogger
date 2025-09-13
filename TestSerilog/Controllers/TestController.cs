using Microsoft.AspNetCore.Mvc;
using SerilogLogger.Abstraction.LoggerInterface;
using TestSerilog.Samples;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TestSerilog.Controllers
{
	
	
	[Route("api/[controller]")]
	[ApiController]
	public class TestController(ILog logger) : ControllerBase
	{
		private readonly ILog _logger = logger;

		// GET: api/<TestController>
		[HttpGet]
		public void Get()
		{
			var usage = new UsageSamples(logger);

			usage.DemonstrateUsage();
		}

		// GET api/<TestController>/5
		[HttpGet("{id}")]
		public string Get(int id)
		{
			return "value";
		}

		// POST api/<TestController>
		[HttpPost]
		public void Post([FromBody] string value)
		{
		}

		// PUT api/<TestController>/5
		[HttpPut("{id}")]
		public void Put(int id, [FromBody] string value)
		{
		}

		// DELETE api/<TestController>/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
		}
	}
}
