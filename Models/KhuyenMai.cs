namespace QuanLyThucAnNhanh.Models
{
    public class KhuyenMai
    {
        public long KhuyenMaiId { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public string? Loai { get; set; }
        public decimal? GiaTri { get; set; }
        public DateTime? BatDau { get; set; }
        public DateTime? KetThuc { get; set; }
        public int? SoLuongToiDa { get; set; }
        public bool TrangThai { get; set; } = true;
    }
}


