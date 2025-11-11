using Microsoft.EntityFrameworkCore;
using QuanLyThucAnNhanh.Models;

namespace QuanLyThucAnNhanh.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<NguoiDung> NguoiDungs { get; set; }
        public DbSet<OTPEmail> OTPEmails { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<NhomMon> NhomMons { get; set; }
        public DbSet<Mon> Mons { get; set; }
        public DbSet<GioHang> GioHangs { get; set; }
        public DbSet<GioHangChiTiet> GioHangChiTiets { get; set; }
        public DbSet<DanhGiaMon> DanhGiaMons { get; set; }
        public DbSet<KhuyenMai> KhuyenMais { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        // 👇 THÊM PHẦN NÀY
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Định nghĩa khóa chính cho OTPEmail
            modelBuilder.Entity<OTPEmail>(entity =>
            {
                entity.ToTable("OTPEmails");         // đúng tên bảng
                entity.HasKey(e => e.OtpId);         // khóa chính
                entity.Property(e => e.OtpId)
                      .HasColumnName("OtpId");       // map đúng tên cột trong SQL
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
