using Common.DAL;

namespace ApplicationRegistry.DomainModels
{
    public class ApplicationRegistryItem : IIDObject
    {
        public int ID { get; set; }
        public string ApplicationName { get; set; }
        public DateTimeOffset FirstInstallDateTime { get; set; }
        public List<ApplicationRegistryItemApplicationRegistryVersion> ApplicationRegistryItemApplicationRegistryVersions { get; set; } = new List<ApplicationRegistryItemApplicationRegistryVersion>();
    }
}
