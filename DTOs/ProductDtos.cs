using System.ComponentModel.DataAnnotations;

namespace QuanLyThucAnNhanh.DTOs
{
	public class CreateProductDto
	{
		public long? idDanhMuc { get; set; }
		public long? IdDanhMuc { get; set; }
		public string? tenSp { get; set; }
		public string? TenSp { get; set; }
		public decimal? gia { get; set; }
		public decimal? Gia { get; set; }
		public bool? TrangThai { get; set; }
		public string? hinh { get; set; }
		public string? Hinh { get; set; }
		public string? moTa { get; set; }
		public string? MoTa { get; set; }
	}

	public class SetPromotionDto
	{
		public decimal? PhanTramGiamGia { get; set; }
		public decimal? SoTienGiamGia { get; set; }
	}

	public class ProductListQuery
	{
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 100;
		public string? TenSp { get; set; }
		public long? IdDanhMuc { get; set; }
		public decimal? GiaMin { get; set; }
		public decimal? GiaMax { get; set; }
	}
}


