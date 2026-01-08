using IoTWebAPI.Data;
using IoTWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // [MỚI] Thêm thư viện phân quyền
using System.Security.Claims; // [MỚI] Thêm thư viện lấy thông tin Token

namespace IoTWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public UsersController(AppDbContext db) => _db = db;

        // CREATE: POST /api/users
        // [Yêu cầu] Chỉ Admin được tạo User mới
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(User user)
        {
            // [SỬA] created_at -> CreatedAt
            user.CreatedAt ??= DateTime.UtcNow;

            // Nếu không nhập Role, mặc định là User
            if (string.IsNullOrEmpty(user.Role)) user.Role = "User";

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // [SỬA] id -> Id
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        // READ ALL: GET /api/users
        // [Yêu cầu] Chỉ Admin được xem danh sách
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _db.Users.AsNoTracking().ToListAsync();
            return Ok(users);
        }

        // READ ONE: GET /api/users/{id}
        // [Yêu cầu] Chỉ Admin được xem chi tiết user khác
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id)
        {
            // [SỬA] id -> Id
            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return user == null ? NotFound() : Ok(user);
        }

        // UPDATE: PUT /api/users/{id}
        // [Yêu cầu] User tự sửa của mình, Admin sửa được hết
        [HttpPut("{id:int}")]
        [Authorize] // Đăng nhập là được gọi, nhưng check quyền bên trong
        public async Task<IActionResult> Update(int id, User input)
        {
            // 1. Lấy Role và ID người đang đăng nhập
            var currentRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // 2. Logic chặn: Nếu KHÔNG PHẢI Admin VÀ ID muốn sửa KHÁC ID của bản thân -> Chặn
            if (currentRole != "Admin" && currentUserId != id.ToString())
            {
                return Forbid(); // Trả về lỗi 403 Forbidden
            }

            // 3. Tìm User trong DB
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null) return NotFound();

            // 4. Cập nhật thông tin (Sửa tên biến thành viết Hoa)
            user.Username = input.Username;
            user.Password = input.Password; // Lưu ý: Thực tế nên hash password
            user.Email = input.Email;

            // Chỉ Admin mới được quyền đổi Role của người khác (tránh user tự nâng quyền lên Admin)
            if (currentRole == "Admin" && !string.IsNullOrEmpty(input.Role))
            {
                user.Role = input.Role;
            }

            await _db.SaveChangesAsync();
            return Ok(user);
        }

        // DELETE: DELETE /api/users/{id}
        // [Yêu cầu] Chỉ Admin được xóa
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null) return NotFound();

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}