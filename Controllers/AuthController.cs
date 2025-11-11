using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;           // <--
using QuanLyThucAnNhanh.Data;                 // <--
using QuanLyThucAnNhanh.DTOs;
using QuanLyThucAnNhanh.Services;

namespace QuanLyThucAnNhanh.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _auth;
        private readonly AppDbContext _db;     // <--

        public AuthController(AuthService auth, AppDbContext db) // <--
        {
            _auth = auth;
            _db = db;                          // <--
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            await _auth.DangKy(dto);
            return Ok("Đăng ký thành công, vui lòng kiểm tra email để lấy OTP");
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(VerifyOtpDto dto)
        {
            var ok = await _auth.XacNhanOtp(dto);
            return ok ? Ok("Xác thực thành công") : BadRequest("OTP sai hoặc đã hết hạn");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var token = await _auth.DangNhap(dto);
            if (token == null) return Unauthorized("Sai tài khoản hoặc mật khẩu");

            // Lấy họ tên để trả kèm
            var user = await _db.NguoiDungs
                                .AsNoTracking()
                                .FirstOrDefaultAsync(u => u.TenDangNhap == dto.TenDangNhap);

            // nên dùng key thường để FE truy cập dễ: res.data.token, res.data.hoTen
            return Ok(new { token, hoTen = user?.HoTen ?? "" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotDto dto)
        {
            await _auth.QuenMatKhau(dto);
            return Ok("OTP đặt lại mật khẩu đã được gửi tới email");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetDto dto)
        {
            var ok = await _auth.ResetMatKhau(dto);
            return ok ? Ok("Đặt lại mật khẩu thành công") : BadRequest("OTP sai hoặc đã hết hạn");
        }
    }
}
