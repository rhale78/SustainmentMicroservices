using Microsoft.AspNetCore.Mvc;
using Queue.Interfaces;

namespace Queue.Microservice.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QueueController : ControllerBase
    {
        public IConfiguration Configuration { get; }
        public ILogger<QueueController> Logger { get; }
        public IQueueService QueueService { get; }

        public QueueController(IConfiguration configuration, ILogger<QueueController> logger,IQueueService queueService)
        {
            Configuration = configuration;
            Logger = logger;
            QueueService = queueService;
        }

        
        //[HttpGet(Name = "GetWeatherForecast")]
        //public IEnumerable<WeatherForecast> Get()
        //{
        //    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //    {
        //        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        //        TemperatureC = Random.Shared.Next(-20, 55),
        //        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        //    })
        //    .ToArray();
        //}
    }
}
