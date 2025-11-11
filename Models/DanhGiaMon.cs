using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThucAnNhanh.Models
{
    [Table("DanhGiaMon")]
    public class DanhGiaMon
    {
        [Key]
        public long DanhGiaMonId { get; set; }

        [Required]
        public long MonId { get; set; }

        [Required]
        public long NguoiDungId { get; set; }

        [Range(1, 5)]
        public int Diem { get; set; }

        [MaxLength(1000)]
        public string? NhanXet { get; set; }

        public DateTime Ngay { get; set; } = DateTime.UtcNow;

        [ForeignKey("MonId")]
        public Mon? Mon { get; set; }

        [ForeignKey("NguoiDungId")]
        public NguoiDung? NguoiDung { get; set; }
    }
}


