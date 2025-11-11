using Microsoft.EntityFrameworkCore;
using QuanLyThucAnNhanh.Data;
using QuanLyThucAnNhanh.DTOs;

namespace QuanLyThucAnNhanh.Services
{
    public class UserAdminService
    {
        private readonly AppDbContext _db;
        public UserAdminService(AppDbContext db) { _db = db; }

        public async Task<List<UserSummaryDto>> GetAllAsync()
        {
            return await _db.NguoiDungs
                .Select(u => new UserSummaryDto
                {
                    NguoiDungId = u.NguoiDungId,
                    TenDangNhap = u.TenDangNhap,
                    Email = u.Email,
                    HoTen = u.HoTen,
                    VaiTro = u.VaiTro,
                    TrangThai = u.TrangThai
                }).ToListAsync();
        }

        public async Task<bool> UpdateRoleAsync(long userId, string role)
        {
            var u = await _db.NguoiDungs.FindAsync(userId);
            if (u == null) return false;
            u.VaiTro = role;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStatusAsync(long userId, bool status)
        {
            var u = await _db.NguoiDungs.FindAsync(userId);
            if (u == null) return false;
            u.TrangThai = status;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResetPasswordAsync(long userId, string newPassword)
        {
            var u = await _db.NguoiDungs.FindAsync(userId);
            if (u == null) return false;
            u.MatKhauBam = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}


