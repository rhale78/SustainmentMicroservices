using ApplicationRegistry.Model;
using ApplicationRegistry.Services;
using Log;
using Microsoft.AspNetCore.Mvc;

namespace ApplicationRegistry.Microservice.Controllers
{
    [ApiController]
    [Route("[controller]/")]
    public class ApplicationRegistryController : ControllerBase
    {
        protected IConfiguration Configuration { get; set; }
        protected IApplicationRegistryService ApplicationRegistryService { get; set; }
        protected LogInstance Log { get; set; } = LogInstance.CreateLog();

        public ApplicationRegistryController(IConfiguration configuration, IApplicationRegistryService applicationRegistryService)
        {
            Configuration = configuration;
            ApplicationRegistryService = applicationRegistryService;
        }

        [HttpPost]
        [HttpPost("Register")]
        public async Task<ActionResult<ApplicationRegistryResult>> Register([FromBody] ApplicationRegistryEntry applicationRegistryEntry)
        {
            if (applicationRegistryEntry == null)
            {
                return BadRequest("ApplicationEntry is null");
            }

            try
            {
                return await ApplicationRegistryService.Register(applicationRegistryEntry).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.LogException(ex);
                return NoContent();
            }
        }

        [HttpPost("Verify")]
        public async Task<ActionResult<ApplicationVerificationStatus>> Verify([FromBody] VerifyApplicationModel applicationRegistryEntry)
        {
            if (applicationRegistryEntry == null)
            {
                return BadRequest("ApplicationEntry is null");
            }

            try
            {
                return new ApplicationVerificationStatus() { StatusType = await ApplicationRegistryService.VerifyApplicationModel(applicationRegistryEntry).ConfigureAwait(false) };
            }
            catch (Exception ex)
            {
                return NoContent();
            }
        }

        [HttpGet("{applicationInstanceID}")]
        public async Task<ActionResult<ApplicationRegistryHierarchyForApplicationInstanceResult>> GetApplicationInstanceHierarchyInfo([FromRoute] int applicationInstanceID)
        {
            if (applicationInstanceID == 0)
            {
                return BadRequest("ApplicationInstanceID is null");
            }

            try
            {
                return await ApplicationRegistryService.GetApplicationRegistryHierarchyInfo(applicationInstanceID).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return NoContent();
            }
        }

        //RSH 2/8/24 - this should be changed to a get with the applicationInstanceID in the route
        [HttpPost("Active")]
        public async Task<ActionResult<ApplicationActiveResult>> IsActive([FromBody] ApplicationActiveRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.FriendlyName) || request.ApplicationInstanceID <= 0)
            {
                return BadRequest("Invalid application active request data");
            }

            try
            {
                return await ApplicationRegistryService.IsActive(request).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return NoContent();
            }
        }

        //RSH 2/8/24 - moved from discovery
        //RSH 2/8/24 - this needs to be more clear what it does via the route - this SETS/updates the IsActive flag
        [HttpPut("InstanceIsActiveFlag")]
        public async Task<ActionResult> SetInstanceIsActiveFlag([FromBody] Model.InstanceIsActive request)
        {
            if (request == null || request.ApplicationInstanceID <= 0)
            {
                return BadRequest("Invalid application active request data");
            }

            try
            {
                await ApplicationRegistryService.SetApplicationActiveFlag(request).ConfigureAwait(false);
                return Ok();
            }
            catch (Exception ex)
            {
                return NoContent();
            }
        }

        [HttpGet("WhoIs/{applicationInstanceID}")]
        public async Task<ActionResult<WhoIsResponse>> WhoIs([FromRoute] int applicationInstanceID)
        {
            if (applicationInstanceID == 0)
            {
                return BadRequest("ApplicationInstanceID is null");
            }

            try
            {
                return await ApplicationRegistryService.WhoIs(applicationInstanceID).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return NoContent();
            }
        }

        [HttpGet("Location/{applicationInstanceID}")]
        public async Task<ActionResult<ApplicationInstanceLocationResponse>> GetInstanceLocation([FromRoute] int applicationInstanceID)
        {
            if (applicationInstanceID == 0)
            {
                return BadRequest("ApplicationInstanceID is null");
            }

            try
            {
                return await ApplicationRegistryService.GetInstanceLocation(applicationInstanceID).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return NoContent();
            }
        }

        //RSH 2/8/24 - could probably just pass this in the route - no real need for a body/class here
        [HttpPut("RegistryInstanceHeartbeat")]
        public async Task<ActionResult> IncrementRegistryInstanceHeartbeat([FromBody] Model.InstanceHeartbeat instanceHeartbeat)
        {
            if (instanceHeartbeat == null || instanceHeartbeat.ApplicationInstanceID <= 0)
            {
                return BadRequest("Invalid instance heartbeat data");
            }

            try
            {
                await ApplicationRegistryService.IncrementRegistryInstanceHeartbeat(instanceHeartbeat.ApplicationInstanceID).ConfigureAwait(false);
                return Ok();
            }
            catch (Exception ex)
            {
                return NoContent();
            }
        }
    }
}
