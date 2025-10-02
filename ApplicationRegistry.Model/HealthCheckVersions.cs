//using ApplicationRegistry.DomainModels;

namespace ApplicationRegistry.Model
{
    public class HealthCheckVersions
    {
        public string ApplicationVersion { get; set; }
        public DateTimeOffset FirstInstallDateTime { get; set; }
        public List<HealthCheckInstances> Instances { get; set; } = new List<HealthCheckInstances>();
        public List<string> FriendlyNames { get; set; } = new List<string>();
        public List<HealthCheckURLs> URLs { get; set; } = new List<HealthCheckURLs>();
    }
}
