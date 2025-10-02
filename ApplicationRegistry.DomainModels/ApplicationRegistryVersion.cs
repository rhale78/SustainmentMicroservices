using Common.DAL;

namespace ApplicationRegistry.DomainModels
{
    public class ApplicationRegistryVersion : IIDObject
    {
        public int ID { get; set; }
        public string ApplicationVersion { get; set; }
        public string ApplicationHash { get; set; }
        public int? PreviousVersionID { get; set; } = 0;
        public ApplicationRegistryVersion PreviousVersion { get; set; }
        public DateTimeOffset BuildDateTime { get; set; }
        public DateTimeOffset FirstInstallDateTime { get; set; }
        public List<ApplicationRegistryVersionApplicationDiscoveryItem> ApplicationRegistryVersionApplicationDiscoveryItems { get; set; } = new List<ApplicationRegistryVersionApplicationDiscoveryItem>();
        public List<ApplicationRegistryVersionApplicationRegistryInstance> ApplicationRegistryVersionApplicationRegistryInstances { get; set; } = new List<ApplicationRegistryVersionApplicationRegistryInstance>();
        public List<ApplicationRegistryItemApplicationRegistryVersion> ApplicationRegistryItemApplicationRegistryVersions { get; set; } = new List<ApplicationRegistryItemApplicationRegistryVersion>();

    }
}
