using Common.DAL;

namespace ApplicationRegistry.DomainModels
{
    public class ApplicationDiscoveryURLApplicationDiscoveryItem
    {
        public int ApplicationDiscoveryURLID { get; set; }
        public ApplicationDiscoveryURL ApplicationDiscoveryURL { get; set; }
        public int ApplicationDiscoveryItemID { get; set; }
        public ApplicationDiscoveryItem ApplicationDiscoveryItem { get; set; }

    }
}
