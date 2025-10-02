using Common.DAL;

namespace ApplicationRegistry.DomainModels
{
    public class ApplicationDiscoveryItem : IIDObject
    {
        public int ID { get; set; }
        public string FriendlyName { get; set; }
        public string ControllerName { get; set; }
        public string ControllerRoute { get; set; }
        public List<ApplicationRegistryVersionApplicationDiscoveryItem> ApplicationRegistryVersionsApplicationDiscoveryItems { get; set; } = new List<ApplicationRegistryVersionApplicationDiscoveryItem>();
        public List<ApplicationDiscoveryURLApplicationDiscoveryItem> ApplicationDiscoveryURLApplicationDiscoveryItems { get; set; } = new List<ApplicationDiscoveryURLApplicationDiscoveryItem>();
        public List<ApplicationDiscoveryMethodApplicationDiscoveryItem> ApplicationDiscoveryMethodsApplicationDiscoveryItems { get; set; } = new List<ApplicationDiscoveryMethodApplicationDiscoveryItem>();
    }
}
