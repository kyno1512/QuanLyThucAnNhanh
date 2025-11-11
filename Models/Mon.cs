using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThucAnNhanh.Models
{
    [Table("Mon")]
    public class Mon
    {
        [Key]
        public long MonId { get; set; }

        [Required]
        public long NhomMonId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Ten { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Gia { get; set; }

        public bool TrangThai { get; set; } = true;

        // Thêm trường lưu URL ảnh (không có trong DB gốc, nhưng nên thêm)
        [MaxLength(500)]
        public string? HinhAnh { get; set; }

        // Mô tả ngắn (phù hợp với cột MoTaNgan trong DB nếu được thêm)
        [MaxLength(200)]
        public string? MoTaNgan { get; set; }

        // Navigation property
        [ForeignKey("NhomMonId")]
        public NhomMon? NhomMon { get; set; }
    }
}

