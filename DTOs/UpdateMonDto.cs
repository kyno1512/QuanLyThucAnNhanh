using System.ComponentModel.DataAnnotations;

namespace QuanLyThucAnNhanh.DTOs
{
    public class UpdateMonDto
    {
        [Required(ErrorMessage = "Nhóm món là bắt buộc")]
        public long NhomMonId { get; set; }

        [Required(ErrorMessage = "Tên món là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Tên món không được quá 100 ký tự")]
        public string Ten { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giá là bắt buộc")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal Gia { get; set; }

        public bool TrangThai { get; set; }

        [MaxLength(500)]
        public string? HinhAnh { get; set; }

        [MaxLength(200)]
        public string? MoTaNgan { get; set; }
    }
}

