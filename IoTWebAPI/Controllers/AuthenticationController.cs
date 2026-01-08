using IoTWebAPI.Data;
using IoTWebAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IoTWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _configuration;

        public AuthenticationController(AppDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }
        
        // 1. API Đăng nhập cho User/Admin
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            User? user = null;

            // Kiểm tra: Nếu là Admin cứng (theo đề bài)
            if (model.Username == "admin" && model.Password == "123456")
            {
                user = new User { Id = 0, Username = "admin", Role = "Admin" };
            }
            else
            {
                // Nếu không phải admin, tìm trong Database
                user = await _db.Users.FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password);
            }

            if (user == null)
            {
                return Unauthorized("Sai tài khoản hoặc mật khẩu");
            }

            // Tạo Token cho User
            var token = GenerateJwtToken(user.Id.ToString(), user.Username, user.Role);
            return Ok(new { token = token });
        }

        // 2. API Xác thực cho Thiết bị (Device)
        [HttpPost("device-auth")]
        public async Task<IActionResult> DeviceAuth([FromBody] DeviceAuthModel model)
        {
            // Tìm thiết bị khớp ID và Key
            var device = await _db.Devices.FirstOrDefaultAsync(d => d.Id == model.DeviceId && d.DeviceKey == model.DeviceKey);

            if (device == null)
            {
                return Unauthorized("Sai thông tin thiết bị");
            }

            // Tạo Token cho Device (Role là "Device")
            var token = GenerateJwtToken(device.Id.ToString(), device.Name, "Device");
            return Ok(new { token = token });
        }

        // Hàm tạo JWT Token chung
        private string GenerateJwtToken(string id, string username, string role)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? "Key_Mac_Dinh_An_Toan_123456789";
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Class phụ để nhận dữ liệu từ Client
        public class LoginModel
        {
            public string Username { get; set; } = "";
            public string Password { get; set; } = "";
        }

        public class DeviceAuthModel
        {
            public int DeviceId { get; set; }
            public string DeviceKey { get; set; } = "";
        }
    }
}