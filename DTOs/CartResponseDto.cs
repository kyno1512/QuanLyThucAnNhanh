namespace QuanLyThucAnNhanh.DTOs
{
    public class CartResponseDto
    {
        public long GioHangId { get; set; }
        public long NguoiDungId { get; set; }
        public DateTime TaoLuc { get; set; }
        public List<CartItemResponseDto> Items { get; set; } = new();
        public decimal TongTien { get; set; } // Tổng tiền tất cả món trong giỏ
        public int TongSoLuong { get; set; } // Tổng số lượng món trong giỏ
    }
}

