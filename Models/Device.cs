using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace IoTWebAPI.Models
{
    public class Device
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = "";
        public string DeviceKey { get; set; } = "";
        public bool IsOnline { get; set; } = false;
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [JsonIgnore]
        public virtual ICollection<SensorData>? SensorDatas { get; set; }
    }
}