//using ApplicationRegistry.DomainModels;

namespace ApplicationRegistry.Model
{
    public class HealthCheckURLs
    {
        public string URL { get; set; }
        public string HealthStatus { get; set; }
        public DateTimeOffset? LastHealthStatusDateTime { get; set; }
    }
}
