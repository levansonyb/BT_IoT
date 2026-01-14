using System.ComponentModel.DataAnnotations.Schema;

namespace IoTWebAPI.Models
{
    public class SensorData
    {
        public int Id { get; set; }
        public int DeviceId { get; set; }
        public string Type { get; set; } = "";
        public double Value { get; set; } = 0.0;
        public DateTime? ReceivedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("DeviceId")]
        public virtual Device? Device { get; set; }
    }
}