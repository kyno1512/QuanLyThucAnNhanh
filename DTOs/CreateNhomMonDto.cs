using System.ComponentModel.DataAnnotations;

namespace QuanLyThucAnNhanh.DTOs
{
    public class CreateNhomMonDto
    {
        [Required(ErrorMessage = "Tên nhóm món là bắt buộc")]
        [MaxLength(100, ErrorMessage = "Tên nhóm món không được quá 100 ký tự")]
        public string Ten { get; set; } = string.Empty;
    }
}

