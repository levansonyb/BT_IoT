using IoTWebAPI.Data;
using IoTWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // [MỚI] Thêm thư viện phân quyền
using System.Security.Claims; // [MỚI] Thêm thư viện lấy thông tin từ Token

namespace IoTWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SensorDataController : ControllerBase
    {
        private readonly AppDbContext _db;
        public SensorDataController(AppDbContext db) => _db = db;

        // [MỚI] Hàm phụ: Kiểm tra quyền sở hữu dữ liệu
        // User chỉ được xem/sửa/xóa nếu thiết bị đó thuộc về mình
        private async Task<bool> CanAccessData(int deviceId)
        {
            if (User.IsInRole("Admin")) return true; // Admin được tất cả
            if (User.IsInRole("Device")) return false; // Device không được xem/sửa/xóa

            // Lấy ID user hiện tại
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdStr, out int userId))
            {
                // Kiểm tra thiết bị có thuộc về user này không
                var device = await _db.Devices.AsNoTracking().FirstOrDefaultAsync(d => d.Id == deviceId);
                return device != null && device.UserId == userId;
            }
            return false;
        }

        // CREATE: POST /api/sensordata
        // [SỬA] Thêm [Authorize]. Device được phép gọi hàm này.
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(SensorData data)
        {
            // [SỬA] device_id -> DeviceId
            var deviceExists = await _db.Devices.AnyAsync(d => d.Id == data.DeviceId);
            if (!deviceExists) return BadRequest($"Device id {data.DeviceId} does not exist.");

            // [SỬA] received_at -> ReceivedAt
            data.ReceivedAt ??= DateTime.UtcNow;

            _db.SensorData.Add(data);
            await _db.SaveChangesAsync();

            // [SỬA] id -> Id
            return CreatedAtAction(nameof(GetById), new { id = data.Id }, data);
        }

        // READ ALL: GET /api/sensordata
        // [SỬA] Chỉ Admin và User được xem. Device không được xem.
        [HttpGet]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetAll()
        {
            var query = _db.SensorData.AsNoTracking().Include(sd => sd.Device).AsQueryable();

            // [MỚI] Nếu là User thường, chỉ lấy data từ các thiết bị của mình
            if (!User.IsInRole("Admin"))
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int.TryParse(userIdStr, out int userId);
                // Lọc dữ liệu theo User sở hữu thiết bị
                query = query.Where(sd => sd.Device.UserId == userId);
            }

            var list = await query.Select(sd => new
            {
                // [SỬA] Sửa hết tên biến thành viết Hoa chữ cái đầu
                sd.Id,
                sd.DeviceId,
                device_name = sd.Device != null ? sd.Device.Name : "",
                sd.Type,
                sd.Value,
                sd.ReceivedAt
            }).ToListAsync();

            return Ok(list);
        }

        // READ ONE: GET /api/sensordata/{id}
        // [SỬA] Chỉ Admin và User được xem.
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _db.SensorData.Include(s => s.Device).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (data == null) return NotFound();

            // [MỚI] Kiểm tra quyền sở hữu
            if (!await CanAccessData(data.DeviceId)) return Forbid();

            return Ok(data);
        }

        // UPDATE: PUT /api/sensordata/{id}
        // [SỬA] Chỉ Admin và User được sửa. Device không được sửa.
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Update(int id, SensorData input)
        {
            var data = await _db.SensorData.FirstOrDefaultAsync(x => x.Id == id);
            if (data == null) return NotFound();

            // [MỚI] Kiểm tra quyền sở hữu trước khi cho sửa
            if (!await CanAccessData(data.DeviceId)) return Forbid();

            // [SỬA] Tên biến viết Hoa
            data.Type = input.Type;
            data.Value = input.Value;
            data.ReceivedAt = input.ReceivedAt ?? DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(data);
        }

        // DELETE: DELETE /api/sensordata/{id}
        // [SỬA] Chỉ Admin và User được xóa.
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _db.SensorData.FirstOrDefaultAsync(x => x.Id == id);
            if (data == null) return NotFound();

            // [MỚI] Kiểm tra quyền sở hữu trước khi xóa
            if (!await CanAccessData(data.DeviceId)) return Forbid();

            _db.SensorData.Remove(data);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // FILTER (Giữ nguyên logic nhưng sửa tên biến và thêm Auth)
        [HttpGet("by-device")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetByDeviceNameAndSeconds([FromQuery] string deviceName, [FromQuery] int seconds = 60)
        {
            if (string.IsNullOrWhiteSpace(deviceName)) return BadRequest("deviceName is required.");
            if (seconds <= 0) return BadRequest("seconds must be > 0.");

            var since = DateTime.UtcNow.AddSeconds(-seconds);

            // Logic lọc nâng cao kết hợp phân quyền
            var query = _db.SensorData
                .AsNoTracking()
                .Include(sd => sd.Device)
                .Where(sd => sd.Device != null
                             && sd.Device.Name == deviceName // [SỬA] name -> Name
                             && (sd.ReceivedAt ?? DateTime.MinValue) >= since); // [SỬA] received_at -> ReceivedAt

            // Nếu là User, check thêm điều kiện sở hữu thiết bị đó
            if (!User.IsInRole("Admin"))
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int.TryParse(userIdStr, out int userId);
                query = query.Where(sd => sd.Device.UserId == userId);
            }

            var list = await query.Select(sd => new
            {
                sd.Id,
                sd.DeviceId,
                device_name = sd.Device!.Name,
                sd.Type,
                sd.Value,
                sd.ReceivedAt
            })
            .OrderByDescending(x => x.ReceivedAt)
            .ToListAsync();

            return Ok(list);
        }
    }
}