using Microsoft.EntityFrameworkCore;
using QuanLyThucAnNhanh.Data;
using QuanLyThucAnNhanh.DTOs;
using QuanLyThucAnNhanh.Models;

namespace QuanLyThucAnNhanh.Services
{
    public class AuthService
    {
        private readonly AppDbContext _db;
        private readonly JwtProvider _jwt;
        private readonly EmailService _email;
        private readonly AuditLogService _log;

        public AuthService(AppDbContext db, JwtProvider jwt, EmailService email, AuditLogService log)
        {
            _db = db;
            _jwt = jwt;
            _email = email;
            _log = log;
        }

        // 🔹 Đăng ký người dùng mới
        public async Task DangKy(RegisterDto dto)
        {
            var user = new NguoiDung
            {
                TenDangNhap = dto.TenDangNhap,
                Email = dto.Email,
                HoTen = dto.HoTen,
                SoDienThoai = dto.SoDienThoai,
                MatKhauBam = BCrypt.Net.BCrypt.HashPassword(dto.MatKhau),
                TrangThai = false
            };

            _db.NguoiDungs.Add(user);
            await _db.SaveChangesAsync();

            string otp = new Random().Next(100000, 999999).ToString();
            var otpEntity = new OTPEmail
            {
                NguoiDungId = user.NguoiDungId,
                MucDich = "DangKy",
                MaOtp = otp,
                HetHanLuc = DateTime.UtcNow.AddMinutes(5)
            };
            _db.OTPEmails.Add(otpEntity);
            await _db.SaveChangesAsync();

            //await _email.SendOtpEmailAsync(user.Email, user.TenDangNhap, otp);
            await _log.LogAsync(user.NguoiDungId, "DangKy", "NguoiDung", user.NguoiDungId);
        }

        // 🔹 Xác nhận OTP đăng ký
        public async Task<bool> XacNhanOtp(VerifyOtpDto dto)
        {
            var user = await _db.NguoiDungs.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return false;

            var otp = await _db.OTPEmails
                .Where(o => o.NguoiDungId == user.NguoiDungId && o.MaOtp == dto.MaOtp && o.MucDich == "DangKy")
                .OrderByDescending(o => o.HetHanLuc)
                .FirstOrDefaultAsync();

            if (otp == null || otp.HetHanLuc < DateTime.UtcNow) return false;

            user.TrangThai = true;
            otp.XacMinhLuc = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _log.LogAsync(user.NguoiDungId, "XacNhanOTP", "NguoiDung", user.NguoiDungId);
            return true;
        }

        // 🔹 Đăng nhập
        public async Task<string?> DangNhap(LoginDto dto)
        {
            var user = await _db.NguoiDungs
                .FirstOrDefaultAsync(u => u.TenDangNhap == dto.TenDangNhap);

            if (user == null)
                return null;

            // So sánh mật khẩu: hỗ trợ legacy mật khẩu lưu plain (không có prefix $2)
            bool isPasswordValid;
            if (!string.IsNullOrWhiteSpace(user.MatKhauBam) && user.MatKhauBam.StartsWith("$2"))
            {
                // BCrypt hash
                isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.MatKhau, user.MatKhauBam);
            }
            else
            {
                // Legacy: so sánh plain text, sau đó nâng cấp thành hash để an toàn
                isPasswordValid = string.Equals(dto.MatKhau, user.MatKhauBam);
                if (isPasswordValid)
                {
                    user.MatKhauBam = BCrypt.Net.BCrypt.HashPassword(dto.MatKhau);
                    await _db.SaveChangesAsync();
                }
            }

            if (!isPasswordValid)
                return null;

            // Sinh JWT token
            var token = _jwt.GenerateToken(user);

            // Ghi log
            await _log.LogAsync(user.NguoiDungId, "DangNhap", "NguoiDung", user.NguoiDungId);

            return token;
        }

        // 🔹 Quên mật khẩu (gửi OTP)
        public async Task QuenMatKhau(ForgotDto dto)
        {
            var user = await _db.NguoiDungs.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return;

            string otp = new Random().Next(100000, 999999).ToString();
            var otpEntity = new OTPEmail
            {
                NguoiDungId = user.NguoiDungId,
                MucDich = "QuenMatKhau",
                MaOtp = otp,
                HetHanLuc = DateTime.UtcNow.AddMinutes(5)
            };
            _db.OTPEmails.Add(otpEntity);
            await _db.SaveChangesAsync();

            await _email.SendOtpEmailAsync(user.Email, user.TenDangNhap, otp);
            await _log.LogAsync(user.NguoiDungId, "QuenMatKhau", "NguoiDung", user.NguoiDungId);
        }

        // 🔹 Đặt lại mật khẩu bằng OTP
        public async Task<bool> ResetMatKhau(ResetDto dto)
        {
            var user = await _db.NguoiDungs.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null) return false;

            var otp = await _db.OTPEmails
                .Where(o => o.NguoiDungId == user.NguoiDungId && o.MaOtp == dto.MaOtp && o.MucDich == "QuenMatKhau")
                .OrderByDescending(o => o.HetHanLuc)
                .FirstOrDefaultAsync();

            if (otp == null || otp.HetHanLuc < DateTime.UtcNow) return false;

            user.MatKhauBam = BCrypt.Net.BCrypt.HashPassword(dto.MatKhauMoi);
            otp.XacMinhLuc = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _log.LogAsync(user.NguoiDungId, "ResetMatKhau", "NguoiDung", user.NguoiDungId);
            return true;
        }
    }
}
