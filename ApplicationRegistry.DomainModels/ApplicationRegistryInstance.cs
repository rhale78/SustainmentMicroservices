using Common.DAL;

namespace ApplicationRegistry.DomainModels
{
    public class ApplicationRegistryInstance : IIDObject
    {
        public int ID { get; set; }
        public string MachineName { get; set; }
        public string ApplicationPath { get; set; }
        public int NumberHeartbeats { get; set; }
        public DateTimeOffset? LastHeartbeatDateTime { get; set; }
        public DateTimeOffset LastStartDateTime { get; set; }
        public DateTimeOffset InstallDateTime { get; set; }
        public bool IsActive { get; set; }
        public List<ApplicationRegistryVersionApplicationRegistryInstance> ApplicationRegistryVersionApplicationRegistryInstances { get; set; } = new List<ApplicationRegistryVersionApplicationRegistryInstance>();
        public List<ApplicationRegistryInstanceApplicationDiscoveryURL> ApplicationRegistryInstancesApplicationDiscoveryURLs { get; set; } = new List<ApplicationRegistryInstanceApplicationDiscoveryURL>();

    }
}
