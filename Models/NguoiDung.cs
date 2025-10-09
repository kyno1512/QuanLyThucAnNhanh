using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThucAnNhanh.Models
{
    [Table("NguoiDung")]
    public class NguoiDung
    {
        public long NguoiDungId { get; set; }
        public string TenDangNhap { get; set; } = string.Empty;
        public string MatKhauBam { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? HoTen { get; set; }
        public string? SoDienThoai { get; set; }
        public string VaiTro { get; set; } = "KhachHang";
        public bool TrangThai { get; set; } = true;
        public DateTime TaoLuc { get; set; } = DateTime.Now;

        public List<OTPEmail>? OTPs { get; set; }
        public List<AuditLog>? Logs { get; set; }
    }
}
