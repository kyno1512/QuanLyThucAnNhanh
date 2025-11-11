namespace QuanLyThucAnNhanh.DTOs
{
    public class MonDetailDto
    {
        public long MonId { get; set; }
        public long NhomMonId { get; set; }
        public string Ten { get; set; } = string.Empty;
        public decimal Gia { get; set; }
        public string? MoTaNgan { get; set; }
        public string? HinhAnhUrl { get; set; }
    }
}


