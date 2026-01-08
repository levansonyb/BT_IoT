using IoTWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace IoTWebAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<SensorData> SensorData { get; set; } // Lưu ý: Tên bảng là SensorData

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cấu hình mối quan hệ: User - Device
            modelBuilder.Entity<User>()
                .HasMany(u => u.Devices)
                .WithOne(d => d.User)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa User thì xóa luôn Device

            // Cấu hình mối quan hệ: Device - SensorData
            modelBuilder.Entity<Device>()
                .HasMany(d => d.SensorDatas) // Phải khớp tên biến trong Model Device.cs
                .WithOne(s => s.Device)
                .HasForeignKey(s => s.DeviceId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa Device thì xóa luôn Data
        }
    }
}