namespace ApplicationRegistry.Model
{
    public class ApplicationDiscoveryEntry
    {
        public List<ApplicationDiscoveryURL> ApplicationDiscoveryURLs { get; set; } = new List<ApplicationDiscoveryURL>();
        public List<ApplicationDiscoveryMethod> ApplicationDiscoveryMethods { get; set; } = new List<ApplicationDiscoveryMethod>();
        public string ControllerName { get; set; }
        public string? ControllerRoute { get; set; }
        public string FriendlyName { get; set; }
        public int ApplicationVersionID { get; set; }
        public int ApplicationInstanceID { get; set; }
    }
}
