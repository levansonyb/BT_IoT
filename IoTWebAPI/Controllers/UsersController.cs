using IoTWebAPI.Data;
using IoTWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IoTWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UsersController(AppDbContext db) => _db = db;

        // CREATE: POST /api/users
        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            user.created_at ??= DateTime.UtcNow;
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = user.id }, user);
        }

        // READ ALL: GET /api/users
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _db.Users.AsNoTracking().ToListAsync();
            return Ok(users);
        }

        // READ ONE: GET /api/users/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.id == id);
            return user == null ? NotFound() : Ok(user);
        }

        // UPDATE: PUT /api/users/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, User input)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.id == id);
            if (user == null) return NotFound();

            user.username = input.username;
            user.password = input.password;
            user.email = input.email;

            await _db.SaveChangesAsync();
            return Ok(user);
        }

        // DELETE: DELETE /api/users/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.id == id);
            if (user == null) return NotFound();

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
