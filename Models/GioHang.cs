using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThucAnNhanh.Models
{
    [Table("GioHang")]
    public class GioHang
    {
        [Key]
        public long GioHangId { get; set; }

        [Required]
        public long NguoiDungId { get; set; }

        public DateTime TaoLuc { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("NguoiDungId")]
        public NguoiDung? NguoiDung { get; set; }

        public List<GioHangChiTiet>? ChiTiets { get; set; }
    }
}

