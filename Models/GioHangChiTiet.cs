using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThucAnNhanh.Models
{
    [Table("GioHang_ChiTiet")]
    public class GioHangChiTiet
    {
        [Key]
        public long GioHangChiTietId { get; set; }

        [Required]
        public long GioHangId { get; set; }

        [Required]
        public long MonId { get; set; }

        [Required]
        public int SoLuong { get; set; }

        // Navigation properties
        [ForeignKey("GioHangId")]
        public GioHang? GioHang { get; set; }

        [ForeignKey("MonId")]
        public Mon? Mon { get; set; }
    }
}

