using System.ComponentModel.DataAnnotations;

namespace IoTWebAPI.Models
{
    public class SensorData
    {
        public int id { get; set; }

        [Required]
        public int device_id { get; set; }

        [Required]
        public string type { get; set; } = "";

        public double value { get; set; } = 0.0;

        public DateTime? received_at { get; set; } = DateTime.UtcNow;

        // Navigation
        public Device? Device { get; set; }
    }
}
