using Common.DAL;

namespace ApplicationRegistry.DomainModels
{
    public class ApplicationRegistryItemApplicationRegistryVersion
    {
        public int ApplicationRegistryItemID { get; set; }
        public ApplicationRegistryItem ApplicationRegistryItem { get; set; }
        public int ApplicationRegistryVersionID { get; set; }
        public ApplicationRegistryVersion ApplicationRegistryVersion { get; set; }

    }
}
