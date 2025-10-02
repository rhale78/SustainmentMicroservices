using Common.DAL;

namespace ApplicationRegistry.DomainModels
{
    public class ApplicationRegistryVersionApplicationRegistryInstance
    {
        public int ApplicationRegistryVersionID { get; set; }
        public ApplicationRegistryVersion ApplicationRegistryVersion { get; set; }
        public int ApplicationRegistryInstanceID { get; set; }
        public ApplicationRegistryInstance ApplicationRegistryInstance { get; set; }
    }
}
