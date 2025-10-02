//using ApplicationRegistry.DomainModels;

namespace ApplicationRegistry.Model
{
    public class HealthCheckInstances
    {
        public string MachineName { get; set; }
        public string ApplicationPath { get; set; }
        public int NumberHeartbeats { get; set; }
        public DateTimeOffset LastHeartbeatDateTime { get; set; }
        public DateTimeOffset LastStartDateTime { get; set; }
        public DateTimeOffset InstallDate { get; set; }
        public bool IsActive { get; set; }
    }
}
