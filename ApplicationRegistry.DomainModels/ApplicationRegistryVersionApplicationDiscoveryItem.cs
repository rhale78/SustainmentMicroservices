using Common.DAL;

namespace ApplicationRegistry.DomainModels
{
    public class ApplicationRegistryVersionApplicationDiscoveryItem
    {
        public int ApplicationRegistryVersionID { get; set; }
        public ApplicationRegistryVersion ApplicationRegistryVersion { get; set; }
        public int ApplicationDiscoveryItemID { get; set; }
        public ApplicationDiscoveryItem ApplicationDiscoveryItem { get; set; }

    }
}
