namespace QuanLyThucAnNhanh.DTOs
{
    // KhuyenMai
    public class KhuyenMaiDto
    {
        public long KhuyenMaiId { get; set; }
        public string Ma { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public string? Loai { get; set; }
        public decimal? GiaTri { get; set; }
        public DateTime? BatDau { get; set; }
        public DateTime? KetThuc { get; set; }
        public int? SoLuongToiDa { get; set; }
        public bool? TrangThai { get; set; }
    }

    public class CreateKhuyenMaiDto
    {
        public string Ma { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public string? Loai { get; set; }
        public decimal? GiaTri { get; set; }
        public DateTime? BatDau { get; set; }
        public DateTime? KetThuc { get; set; }
        public int? SoLuongToiDa { get; set; }
        public bool? TrangThai { get; set; }
    }

    // Users
    public class UserSummaryDto
    {
        public long NguoiDungId { get; set; }
        public string TenDangNhap { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? HoTen { get; set; }
        public string VaiTro { get; set; } = "KhachHang";
        public bool TrangThai { get; set; }
    }

    public class UpdateUserRoleDto
    {
        public string VaiTro { get; set; } = "KhachHang";
    }

    public class UpdateUserStatusDto
    {
        public bool TrangThai { get; set; }
    }

    public class ResetUserPasswordDto
    {
        public string MatKhauMoi { get; set; } = string.Empty;
    }

    // Nhap/Xuat kho
    public class ImportItemDto
    {
        public long NguyenLieuId { get; set; }
        public decimal SoLuong { get; set; }
        public decimal? DonGia { get; set; }
        public DateTime? HanSuDung { get; set; }
    }

    public class CreatePhieuNhapDto
    {
        public long ChiNhanhId { get; set; }
        public long? NguoiDungId { get; set; }
        public List<ImportItemDto> Items { get; set; } = new();
    }

    public class ExportItemDto
    {
        public long NguyenLieuId { get; set; }
        public decimal SoLuong { get; set; }
    }

    public class CreatePhieuXuatDto
    {
        public long ChiNhanhId { get; set; }
        public long? NguoiDungId { get; set; }
        public string? LyDo { get; set; }
        public List<ExportItemDto> Items { get; set; } = new();
    }

    // Report
    public class StockItemDto
    {
        public long NguyenLieuId { get; set; }
        public string Ten { get; set; } = string.Empty;
        public decimal SoLuong { get; set; }
    }
}


