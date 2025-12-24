using IoTWebAPI.Data;
using IoTWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IoTWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly AppDbContext _db;
        public DevicesController(AppDbContext db) => _db = db;

        // CREATE: POST /api/devices
        [HttpPost]
        public async Task<IActionResult> Create(Device device)
        {
            // Check user exists
            var userExists = await _db.Users.AnyAsync(u => u.id == device.user_id);
            if (!userExists) return BadRequest($"User id {device.user_id} does not exist.");

            device.created_at ??= DateTime.UtcNow;
            _db.Devices.Add(device);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = device.id }, device);
        }

        // READ ALL (kèm tên người tạo): GET /api/devices
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var devices = await _db.Devices
                .AsNoTracking()
                .Include(d => d.User)
                .Select(d => new
                {
                    d.id,
                    d.user_id,
                    creator_username = d.User != null ? d.User.username : "",
                    d.name,
                    d.device_key,
                    d.is_online,
                    d.created_at
                })
                .ToListAsync();

            return Ok(devices);
        }

        // READ ONE: GET /api/devices/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var device = await _db.Devices.AsNoTracking().FirstOrDefaultAsync(x => x.id == id);
            return device == null ? NotFound() : Ok(device);
        }

        // UPDATE: PUT /api/devices/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Device input)
        {
            var device = await _db.Devices.FirstOrDefaultAsync(x => x.id == id);
            if (device == null) return NotFound();

            // user_id thường không nên đổi; nếu muốn cho đổi thì phải check tồn tại
            device.name = input.name;
            device.device_key = input.device_key;
            device.is_online = input.is_online;

            await _db.SaveChangesAsync();
            return Ok(device);
        }

        // DELETE: DELETE /api/devices/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var device = await _db.Devices.FirstOrDefaultAsync(x => x.id == id);
            if (device == null) return NotFound();

            _db.Devices.Remove(device);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // SEARCH by creator username (query): GET /api/devices/search?creatorUsername=abc
        [HttpGet("search")]
        public async Task<IActionResult> SearchByCreator([FromQuery] string creatorUsername)
        {
            if (string.IsNullOrWhiteSpace(creatorUsername))
                return BadRequest("creatorUsername is required.");

            var devices = await _db.Devices
                .AsNoTracking()
                .Include(d => d.User)
                .Where(d => d.User != null && d.User.username.Contains(creatorUsername))
                .Select(d => new
                {
                    d.id,
                    d.user_id,
                    creator_username = d.User!.username,
                    d.name,
                    d.device_key,
                    d.is_online,
                    d.created_at
                })
                .ToListAsync();

            return Ok(devices);
        }

        // OFFLINE list: GET /api/devices/offline
        [HttpGet("offline")]
        public async Task<IActionResult> GetOfflineDevices()
        {
            var devices = await _db.Devices
                .AsNoTracking()
                .Include(d => d.User)
                .Where(d => d.is_online == false)
                .Select(d => new
                {
                    d.id,
                    d.user_id,
                    creator_username = d.User != null ? d.User.username : "",
                    d.name,
                    d.device_key,
                    d.is_online,
                    d.created_at
                })
                .ToListAsync();

            return Ok(devices);
        }
    }
}
