//using ApplicationRegistry.DomainModels;

namespace ApplicationRegistry.Model
{
    public class ApplicationDiscoveryHealthStatusResult
    {
        public string ApplicationName { get; set; }
        public DateTime FirstInstallDateTime { get; set; }
        public List<HealthCheckVersions> Versions { get; set; } = new List<HealthCheckVersions>();
    }
}
