namespace ApplicationRegistry.Model
{
    public class ApplicationRegistryResult
    {
        public int ApplicationInstanceID { get; set; }
        public bool UpgradeOrCreate { get; set; }
        public int ApplicationID { get; set; }
        public int ApplicationVersionID { get; set; }
    }
}
