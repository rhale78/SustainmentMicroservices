
namespace ApplicationRegistry.Model
{
    public class ApplicationRegistryHierarchyForApplicationInstanceResult
    {
        public int CurrentInstanceID { get; set; }
        public int CurrentVersionID { get; set; }
        public int RegistryID { get; set; }
        public IEnumerable<int> PreviousVersionIDs { get; set; }
    }
}
