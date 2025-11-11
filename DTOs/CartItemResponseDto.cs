namespace QuanLyThucAnNhanh.DTOs
{
    public class CartItemResponseDto
    {
        public long GioHangChiTietId { get; set; }
        public long MonId { get; set; }
        public string TenMon { get; set; } = string.Empty;
        public decimal GiaMon { get; set; }
        public string? HinhAnhMon { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien { get; set; } // GiaMon * SoLuong
    }
}

