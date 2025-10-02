using Common.DAL;

namespace ApplicationRegistry.DomainModels
{
    public class ApplicationDiscoveryMethodApplicationDiscoveryItem
    {
        public int ApplicationDiscoveryMethodID { get; set; }
        public ApplicationDiscoveryMethod ApplicationDiscoveryMethod { get; set; }
        public int ApplicationDiscoveryItemID { get; set; }
        public ApplicationDiscoveryItem ApplicationDiscoveryItem { get; set; }

    }
}
