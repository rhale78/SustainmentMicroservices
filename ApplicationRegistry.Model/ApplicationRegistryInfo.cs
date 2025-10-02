//using ApplicationRegistry.DomainModels;

namespace ApplicationRegistry.Model
{
    public class ApplicationRegistryInfo
    {
        public DomainModels.ApplicationRegistryItem ApplicationRegistryItem { get; set; }
        public DomainModels.ApplicationRegistryVersion ApplicationRegistryVersion { get; set; }
        public DomainModels.ApplicationRegistryInstance ApplicationRegistryInstance { get; set; }
        public bool NewApplicationInstance { get; set; }
        public bool NewApplicationVersion { get; set; }
        public bool NewApplicationItem { get; set; }
    }
}
