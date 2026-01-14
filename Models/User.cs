using System.ComponentModel.DataAnnotations;

namespace IoTWebAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "User";
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public virtual ICollection<Device>? Devices { get; set; }
    }
}