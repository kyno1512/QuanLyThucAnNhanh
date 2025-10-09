using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThucAnNhanh.Models
{
    [Table("AuditLog")] // 🔥 EF sẽ dùng đúng tên bảng trong SQL Server
    public class AuditLog
    {
        [Key]
        public long AuditId { get; set; }   // Trùng với SQL

        public long? NguoiDungId { get; set; }
        public string HanhDong { get; set; } = string.Empty;
        public string? Bang { get; set; }
        public long? BanGhiId { get; set; }
        public string? NoiDung { get; set; }
        public DateTime ThoiGian { get; set; } = DateTime.Now;
    }
}
