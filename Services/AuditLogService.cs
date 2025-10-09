using QuanLyThucAnNhanh.Data;
using QuanLyThucAnNhanh.Models;

namespace QuanLyThucAnNhanh.Services
{
    public class AuditLogService
    {
        private readonly AppDbContext _db;
        public AuditLogService(AppDbContext db) => _db = db;

        public async Task LogAsync(long? userId, string hanhDong, string bang, long? banGhiId)
        {
            var log = new AuditLog
            {
                NguoiDungId = userId,
                HanhDong = hanhDong,
                Bang = bang,
                BanGhiId = banGhiId,
                ThoiGian = DateTime.Now
            };

            _db.AuditLogs.Add(log);
            await _db.SaveChangesAsync();
        }
    }
}
