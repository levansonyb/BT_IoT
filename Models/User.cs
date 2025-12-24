using System.ComponentModel.DataAnnotations;

namespace IoTWebAPI.Models
{
    public class User
    {
        public int id { get; set; }

        [Required]
        public string username { get; set; } = "";

        [Required]
        public string password { get; set; } = "";

        [Required]
        public string email { get; set; } = "";

        public DateTime? created_at { get; set; } = DateTime.UtcNow;

        // Navigation: 1 user -> many devices
        public ICollection<Device>? Devices { get; set; }
    }
}
