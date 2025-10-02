using ApplicationRegistry.Model;
using ApplicationRegistry.Services;
using Log;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationRegistry.Microservice.Controllers
{
    [ApiController]
    [Route("[controller]/")]
    public class ApplicationDiscoveryController : ControllerBase
    {
        protected IConfiguration Configuration { get; set; }
        protected IApplicationDiscoveryService ApplicationDiscoveryService { get; set; }
        protected LogInstance Log { get; set; } = LogInstance.CreateLog();

        public ApplicationDiscoveryController(IConfiguration configuration, IApplicationDiscoveryService applicationDiscoveryService)
        {
            Configuration = configuration;
            ApplicationDiscoveryService = applicationDiscoveryService;
        }

        [HttpGet("URLs/{friendlyName}")]
        public async Task<ActionResult<List<string>>> GetURLsForFriendlyName([FromRoute] string friendlyName)
        {
            if (string.IsNullOrEmpty(friendlyName))
            {
                return BadRequest("FriendlyName is null");
            }
            try
            {
                bool limitToHttps = Configuration.GetBoolValueWithDefault("ForceDiscoveryHttpsOnly", true);
                return await ApplicationDiscoveryService.GetURLsForFriendlyName(friendlyName, limitToHttps).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return NoContent();
            }
        }

        [HttpGet("Routes/{friendlyName}")]
        public async Task<ActionResult<Model.ApplicationDiscoveryRoutes>> GetRoutes([FromRoute] string friendlyName)
        {
            if (string.IsNullOrEmpty(friendlyName))
            {
                return BadRequest("FriendlyName is null");
            }

            try
            {
                return await ApplicationDiscoveryService.GetRoutes(friendlyName).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return NoContent();
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddDiscoveryRecords([FromBody] ApplicationDiscoveryEntry applicationDiscoveryEntry)
        {
            if (applicationDiscoveryEntry is null)
            {
                return BadRequest("ApplicationDiscoveryEntry is null");
            }

            try
            {
                await ApplicationDiscoveryService.AddDiscoveryRecords(applicationDiscoveryEntry).ConfigureAwait(false);
                return Ok();
            }
            catch (Exception ex)
            {
                return NoContent();
            }
        }


        [HttpGet("ApplicationHeathStatus")]
        public async Task<ActionResult<List<Model.ApplicationDiscoveryHealthStatusResult>>> GetAllApplicationHealthStatus()
        {
            try
            {
                return await ApplicationDiscoveryService.GetAllApplicationHealthStatus().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return NoContent();
            }
        }

        [HttpGet("FriendlyNameHealthStatus/{friendlyName}")]
        public async Task<ActionResult<List<Model.ApplicationDiscoveryHealthStatusResult>>> GetApplicationHealthStatusForFriendlyName([FromRoute] string friendlyName)
        {
            if (string.IsNullOrEmpty(friendlyName))
            {
                return BadRequest("FriendlyName is null");
            }
            try
            {
                return await ApplicationDiscoveryService.GetApplicationHealthStatusByFriendlyName(friendlyName).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return NoContent();
            }
        }

        [HttpGet("LatestApplicationHealthStatus")]
        public async Task<ActionResult<List<Model.ApplicationDiscoveryHealthStatusResult>>> GetApplicationHealthStatusByLatest()
        {
            try
            {
                return await ApplicationDiscoveryService.GetApplicationHealthStatusByLatest().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return NoContent();
            }
        }
    }
}