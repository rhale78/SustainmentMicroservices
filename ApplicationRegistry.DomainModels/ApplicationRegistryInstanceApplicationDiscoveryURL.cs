using Common.DAL;

namespace ApplicationRegistry.DomainModels
{
    public class ApplicationRegistryInstanceApplicationDiscoveryURL
    {
        public int ApplicationRegistryInstanceID { get; set; }
        public ApplicationRegistryInstance ApplicationRegistryInstance { get; set; }
        public int ApplicationDiscoveryURLID { get; set; }
        public ApplicationDiscoveryURL ApplicationDiscoveryURL { get; set; }

    }
}
