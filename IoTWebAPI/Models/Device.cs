using System.ComponentModel.DataAnnotations;

namespace IoTWebAPI.Models
{
    public class Device
    {
        public int id { get; set; }

        [Required]
        public int user_id { get; set; } // creator user id

        [Required]
        public string name { get; set; } = "";

        [Required]
        public string device_key { get; set; } = "";

        public bool is_online { get; set; } = false;

        public DateTime? created_at { get; set; } = DateTime.UtcNow;

        // Navigation
        public User? User { get; set; }
        public ICollection<SensorData>? SensorDataList { get; set; }
    }
}
