namespace QuanLyThucAnNhanh.DTOs
{
    public class MonResponseDto
    {
        public long MonId { get; set; }
        public long NhomMonId { get; set; }
        public string TenNhomMon { get; set; } = string.Empty;
        public string Ten { get; set; } = string.Empty;
        public decimal Gia { get; set; }
        public bool TrangThai { get; set; }
        public string? HinhAnh { get; set; }
        public string? MoTaNgan { get; set; }
    }
}

