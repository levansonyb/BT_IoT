using IoTWebAPI.Data;
using IoTWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IoTWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SensorDataController : ControllerBase
    {
        private readonly AppDbContext _db;
        public SensorDataController(AppDbContext db) => _db = db;

        // CREATE: POST /api/sensordata
        [HttpPost]
        public async Task<IActionResult> Create(SensorData data)
        {
            var deviceExists = await _db.Devices.AnyAsync(d => d.id == data.device_id);
            if (!deviceExists) return BadRequest($"Device id {data.device_id} does not exist.");

            data.received_at ??= DateTime.UtcNow;
            _db.SensorData.Add(data);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = data.id }, data);
        }

        // READ ALL (kèm tên thiết bị): GET /api/sensordata
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.SensorData
                .AsNoTracking()
                .Include(sd => sd.Device)
                .Select(sd => new
                {
                    sd.id,
                    sd.device_id,
                    device_name = sd.Device != null ? sd.Device.name : "",
                    sd.type,
                    sd.value,
                    sd.received_at
                })
                .ToListAsync();

            return Ok(list);
        }

        // READ ONE: GET /api/sensordata/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _db.SensorData.AsNoTracking().FirstOrDefaultAsync(x => x.id == id);
            return data == null ? NotFound() : Ok(data);
        }

        // UPDATE: PUT /api/sensordata/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, SensorData input)
        {
            var data = await _db.SensorData.FirstOrDefaultAsync(x => x.id == id);
            if (data == null) return NotFound();

            // device_id thường không nên đổi; nếu đổi cần check tồn tại
            data.type = input.type;
            data.value = input.value;
            data.received_at = input.received_at ?? DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(data);
        }

        // DELETE: DELETE /api/sensordata/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _db.SensorData.FirstOrDefaultAsync(x => x.id == id);
            if (data == null) return NotFound();

            _db.SensorData.Remove(data);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // FILTER by device name + seconds:
        // GET /api/sensordata/by-device?deviceName=ESP32&seconds=60
        [HttpGet("by-device")]
        public async Task<IActionResult> GetByDeviceNameAndSeconds([FromQuery] string deviceName, [FromQuery] int seconds = 60)
        {
            if (string.IsNullOrWhiteSpace(deviceName))
                return BadRequest("deviceName is required.");

            if (seconds <= 0) return BadRequest("seconds must be > 0.");

            var since = DateTime.UtcNow.AddSeconds(-seconds);

            var list = await _db.SensorData
                .AsNoTracking()
                .Include(sd => sd.Device)
                .Where(sd => sd.Device != null
                             && sd.Device.name == deviceName
                             && (sd.received_at ?? DateTime.MinValue) >= since)
                .Select(sd => new
                {
                    sd.id,
                    sd.device_id,
                    device_name = sd.Device!.name,
                    sd.type,
                    sd.value,
                    sd.received_at
                })
                .OrderByDescending(x => x.received_at)
                .ToListAsync();

            return Ok(list);
        }
    }
}
