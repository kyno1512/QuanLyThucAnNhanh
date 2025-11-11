using System.ComponentModel.DataAnnotations;

namespace QuanLyThucAnNhanh.DTOs
{
    public class AddToCartDto
    {
        [Required(ErrorMessage = "Mã món là bắt buộc")]
        public long MonId { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SoLuong { get; set; } = 1;
    }
}

