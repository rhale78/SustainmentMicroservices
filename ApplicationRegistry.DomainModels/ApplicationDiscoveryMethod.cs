using Common.DAL;

namespace ApplicationRegistry.DomainModels
{
    public class ApplicationDiscoveryMethod : IIDObject
    {
        public int ID { get; set; }
        public string HttpMethod { get; set; }
        public string MethodName { get; set; }
        public string Template { get; set; }
        public List<ApplicationDiscoveryMethodApplicationDiscoveryItem> ApplicationDiscoveryMethodsApplicationDiscoveryItems { get; set; } = new List<ApplicationDiscoveryMethodApplicationDiscoveryItem>();

    }
}
