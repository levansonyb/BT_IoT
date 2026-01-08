using IoTWebAPI.Data;
using IoTWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // Cần thiết để dùng [Authorize]
using System.Security.Claims; // Cần thiết để lấy thông tin từ Token

namespace IoTWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Yêu cầu phải có Token mới được gọi API này
    public class DevicesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public DevicesController(AppDbContext db) => _db = db;

        // Hàm phụ: Lấy ID của user đang đăng nhập từ Token
        private int GetCurrentUserId()
        {
            var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idStr, out int id) ? id : 0;
        }

        // Hàm phụ: Kiểm tra xem user đang đăng nhập có phải Admin không
        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }



        // CREATE: POST /api/devices
        [HttpPost]
        public async Task<IActionResult> Create(Device device)
        {
            // Nếu là User thường (không phải Admin), tự động gán thiết bị này cho chính họ
            if (!IsAdmin())
            {
                device.UserId = GetCurrentUserId();
            }

            // Kiểm tra User sở hữu có tồn tại không
            var userExists = await _db.Users.AnyAsync(u => u.Id == device.UserId);
            if (!userExists) return BadRequest($"User id {device.UserId} does not exist.");

            device.CreatedAt ??= DateTime.UtcNow;

            _db.Devices.Add(device);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = device.Id }, device);
        }

        // READ ALL: GET /api/devices
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var query = _db.Devices.AsNoTracking().Include(d => d.User).AsQueryable();

            // Nếu không phải Admin, lọc chỉ lấy thiết bị của chính mình
            if (!IsAdmin())
            {
                var myId = GetCurrentUserId();
                query = query.Where(d => d.UserId == myId);
            }

            var devices = await query.Select(d => new
            {
                d.Id,
                d.UserId,
                // Sửa lỗi chính tả: username -> Username
                creator_username = d.User != null ? d.User.Username : "",
                d.Name,
                d.DeviceKey,
                d.IsOnline,
                d.CreatedAt
            }).ToListAsync();

            return Ok(devices);
        }

        // READ ONE: GET /api/devices/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var device = await _db.Devices.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

            if (device == null) return NotFound();

            // Logic bảo mật: Nếu không phải Admin và thiết bị này KHÔNG PHẢI của mình -> Chặn
            if (!IsAdmin() && device.UserId != GetCurrentUserId())
            {
                return Forbid(); // Trả về lỗi 403 Forbidden
            }

            return Ok(device);
        }

        // UPDATE: PUT /api/devices/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Device input)
        {
            var device = await _db.Devices.FirstOrDefaultAsync(x => x.Id == id);

            if (device == null) return NotFound();

            // Logic bảo mật: Chỉ chủ sở hữu hoặc Admin mới được sửa
            if (!IsAdmin() && device.UserId != GetCurrentUserId())
            {
                return Forbid();
            }

            // Cập nhật dữ liệu (Sửa tên biến thành PascalCase)
            device.Name = input.Name;
            device.DeviceKey = input.DeviceKey;
            device.IsOnline = input.IsOnline;

            await _db.SaveChangesAsync();
            return Ok(device);
        }

        // DELETE: DELETE /api/devices/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var device = await _db.Devices.FirstOrDefaultAsync(x => x.Id == id);

            if (device == null) return NotFound();

            // Logic bảo mật: Chỉ chủ sở hữu hoặc Admin mới được xóa
            if (!IsAdmin() && device.UserId != GetCurrentUserId())
            {
                return Forbid();
            }

            _db.Devices.Remove(device);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // SEARCH (Đã sửa lỗi chính tả)
        [HttpGet("search")]
        public async Task<IActionResult> SearchByCreator([FromQuery] string creatorUsername)
        {
            if (string.IsNullOrWhiteSpace(creatorUsername))
                return BadRequest("creatorUsername is required.");

            var query = _db.Devices.AsNoTracking().Include(d => d.User).AsQueryable();

            // Áp dụng bộ lọc bảo mật trước khi tìm kiếm
            if (!IsAdmin())
            {
                var myId = GetCurrentUserId();
                query = query.Where(d => d.UserId == myId);
            }

            var devices = await query
                .Where(d => d.User != null && d.User.Username.Contains(creatorUsername))
                .Select(d => new
                {
                    d.Id,
                    d.UserId,
                    creator_username = d.User!.Username,
                    d.Name,
                    d.DeviceKey,
                    d.IsOnline,
                    d.CreatedAt
                })
                .ToListAsync();

            return Ok(devices);
        }

        // OFFLINE LIST (Đã sửa lỗi chính tả)
        [HttpGet("offline")]
        public async Task<IActionResult> GetOfflineDevices()
        {
            var query = _db.Devices.AsNoTracking().Include(d => d.User).Where(d => d.IsOnline == false);

            // Áp dụng bộ lọc bảo mật
            if (!IsAdmin())
            {
                var myId = GetCurrentUserId();
                query = query.Where(d => d.UserId == myId);
            }

            var devices = await query
                .Select(d => new
                {
                    d.Id,
                    d.UserId,
                    creator_username = d.User != null ? d.User.Username : "",
                    d.Name,
                    d.DeviceKey,
                    d.IsOnline,
                    d.CreatedAt
                })
                .ToListAsync();

            return Ok(devices);
        }
    }
}