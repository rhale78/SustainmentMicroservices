namespace ApplicationRegistry.Model
{
    public class ApplicationDiscoveryInfo
    {
        public bool NewApplicationDiscoveryItem { get; set; }
        public DomainModels.ApplicationDiscoveryItem ApplicationDiscoveryItem { get; set; }
        public List<(bool isNew, DomainModels.ApplicationDiscoveryURL)> ApplicationDiscoveryURLs { get; set; } = new List<(bool isNew, DomainModels.ApplicationDiscoveryURL)>();
        public List<(bool isNew, DomainModels.ApplicationDiscoveryMethod applicationDiscoveryMethod)> ApplicationDiscoveryMethods { get; set; } = new List<(bool isNew, DomainModels.ApplicationDiscoveryMethod applicationDiscoveryMethod)>();
    }
}
