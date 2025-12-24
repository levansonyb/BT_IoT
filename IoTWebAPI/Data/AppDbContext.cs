using Microsoft.EntityFrameworkCore;
using IoTWebAPI.Models;

namespace IoTWebAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Device> Devices => Set<Device>();
        public DbSet<SensorData> SensorData => Set<SensorData>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User (1) -> (N) Device
            modelBuilder.Entity<Device>()
                .HasOne(d => d.User)
                .WithMany(u => u.Devices)
                .HasForeignKey(d => d.user_id)
                .OnDelete(DeleteBehavior.Cascade);

            // Device (1) -> (N) SensorData
            modelBuilder.Entity<SensorData>()
                .HasOne(sd => sd.Device)
                .WithMany(d => d.SensorDataList)
                .HasForeignKey(sd => sd.device_id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
