using Common.DAL;

namespace ApplicationRegistry.DomainModels
{
    public class ApplicationDiscoveryURL : IIDObject
    {
        public int ID { get; set; }
        public string URL { get; set; }
        public int? Port { get; set; }
        //public int? ApplicationDiscoveryItemID { get; set; }
        //public ApplicationDiscoveryItem ApplicationDiscoveryItem { get; set; }
        public string HealthStatus { get; set; } = string.Empty;
        public DateTimeOffset? LastHealthStatusCheckDateTime { get; set; } = DateTimeOffset.MinValue;
        public List<ApplicationRegistryInstanceApplicationDiscoveryURL> ApplicationRegistryInstancesApplicationDiscoveryURLs { get; set; } = new List<ApplicationRegistryInstanceApplicationDiscoveryURL>();
        public List<ApplicationDiscoveryURLApplicationDiscoveryItem> ApplicationDiscoveryURLApplicationDiscoveryItems { get; set; } = new List<ApplicationDiscoveryURLApplicationDiscoveryItem>();

    }
}
