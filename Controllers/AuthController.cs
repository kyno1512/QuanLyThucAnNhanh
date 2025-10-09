using Microsoft.AspNetCore.Mvc;
using QuanLyThucAnNhanh.DTOs;
using QuanLyThucAnNhanh.Services;

namespace QuanLyThucAnNhanh.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _auth;

        public AuthController(AuthService auth)
        {
            _auth = auth;
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
            return token == null ? Unauthorized("Sai tài khoản hoặc mật khẩu") : Ok(new { Token = token });
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
