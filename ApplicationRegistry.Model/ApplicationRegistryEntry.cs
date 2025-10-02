using System.ComponentModel.DataAnnotations;

namespace ApplicationRegistry.Model
{
    public class ApplicationRegistryEntry
    {
        [Required]
        public string ApplicationName { get; set; }
        [Required]
        public string ApplicationVersion { get; set; }
        [Required]
        public string ApplicationPath { get; set; }
        [Required]
        public string MachineName { get; set; }
        [Required]
        public string ApplicationHash { get; set; }
        public DateTime BuildDateTime { get; set; }
    }
}
