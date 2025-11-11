using QuanLyThucAnNhanh.DTOs;

namespace QuanLyThucAnNhanh.Services
{
	public class ProductService
	{
		private readonly MonService _monService;
		private readonly PromotionService _promotionService;

		public ProductService(MonService monService, PromotionService promotionService)
		{
			_monService = monService;
			_promotionService = promotionService;
		}

		// Danh sách sản phẩm (filter + phân trang) theo format frontend
		public async Task<object> GetProductsAsync(ProductListQuery query)
		{
			try
			{
				var allMons = await _monService.GetAllMonAsync();

				var filtered = allMons.AsEnumerable();

				if (!string.IsNullOrWhiteSpace(query.TenSp))
				{
					var q = query.TenSp.ToLower();
					filtered = filtered.Where(m => (m.Ten ?? string.Empty).ToLower().Contains(q));
				}
				if (query.IdDanhMuc.HasValue)
				{
					filtered = filtered.Where(m => m.NhomMonId == query.IdDanhMuc.Value);
				}
				if (query.GiaMin.HasValue)
				{
					filtered = filtered.Where(m => m.Gia >= query.GiaMin.Value);
				}
				if (query.GiaMax.HasValue)
				{
					filtered = filtered.Where(m => m.Gia <= query.GiaMax.Value);
				}

				var filteredList = filtered.ToList();
				var hotLookup = await _promotionService.GetHotDealPercentLookupAsync(filteredList.Select(m => m.MonId));
				var totalCount = filteredList.Count;
				var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

				var pageItems = filteredList
					.Skip((query.Page - 1) * query.PageSize)
					.Take(query.PageSize)
					.Select(m =>
					{
						hotLookup.TryGetValue(m.MonId, out var percent);
						return MapMonToFrontendProduct(m, percent);
					})
					.ToList();

				return new
				{
					Products = pageItems,
					TotalCount = totalCount,
					Page = query.Page,
					PageSize = query.PageSize,
					TotalPages = totalPages
				};
			}
			catch (Exception ex)
			{
				throw new Exception($"Lỗi trong GetProductsAsync: {ex.Message}", ex);
			}
		}

	// Danh mục theo format frontend
	public async Task<object> GetCategoriesAsync()
	{
		try
		{
			var groups = await _monService.GetAllNhomMonAsync();
			var categories = groups.Select(n => new
			{
				idDanhMuc = n.NhomMonId,
				IdDanhMuc = n.NhomMonId,
				tenDanhMuc = n.Ten ?? string.Empty,
				TenDanhMuc = n.Ten ?? string.Empty
			}).ToList();
			return categories;
		}
		catch (Exception ex)
		{
			throw new Exception($"Lỗi trong GetCategoriesAsync: {ex.Message}", ex);
		}
	}

		// Chi tiết sản phẩm
		public async Task<object?> GetProductAsync(long id)
		{
			var mon = await _monService.GetMonByIdAsync(id);
			if (mon == null) return null;
			var dict = await _promotionService.GetHotDealPercentLookupAsync(new[] { mon.MonId });
			dict.TryGetValue(mon.MonId, out var percent);
			return MapMonToFrontendProduct(mon, percent);
		}

		// Tạo
		public async Task<object> CreateProductAsync(CreateProductDto dto)
		{
			var monDto = new CreateMonDto
			{
				NhomMonId = dto.IdDanhMuc ?? dto.idDanhMuc ?? 0,
				Ten = dto.TenSp ?? dto.tenSp ?? string.Empty,
				Gia = dto.Gia ?? dto.gia ?? 0,
				TrangThai = dto.TrangThai ?? true,
				HinhAnh = dto.Hinh ?? dto.hinh,
				MoTaNgan = dto.MoTa ?? dto.moTa
			};
			var mon = await _monService.CreateMonAsync(monDto);
			return MapMonToFrontendProduct(mon, null);
		}

		// Cập nhật
		public async Task<object?> UpdateProductAsync(long id, CreateProductDto dto)
		{
			var monDto = new UpdateMonDto
			{
				NhomMonId = dto.IdDanhMuc ?? dto.idDanhMuc ?? 0,
				Ten = dto.TenSp ?? dto.tenSp ?? string.Empty,
				Gia = dto.Gia ?? dto.gia ?? 0,
				TrangThai = dto.TrangThai ?? true,
				HinhAnh = dto.Hinh ?? dto.hinh,
				MoTaNgan = dto.MoTa ?? dto.moTa
			};
			var mon = await _monService.UpdateMonAsync(id, monDto);
			if (mon == null) return null;
			var dict = await _promotionService.GetHotDealPercentLookupAsync(new[] { mon.MonId });
			dict.TryGetValue(mon.MonId, out var percent);
			return MapMonToFrontendProduct(mon, percent);
		}

		// Xóa
		public async Task<bool> DeleteProductAsync(long id)
		{
			return await _monService.DeleteMonAsync(id);
		}

		// Khuyến mãi (placeholder)
		public Task<object> SetPromotionAsync(long id, SetPromotionDto dto)
		{
			// Chưa có model lưu khuyến mãi gắn với món; trả OK tạm thời
			return Task.FromResult<object>(new { message = "Thiết lập khuyến mãi thành công" });
		}

		// Test database connection
		public async Task<object> TestDatabaseConnectionAsync()
		{
			try
			{
				var mons = await _monService.GetAllMonAsync();
				var monCount = mons.Count;
				var nhomMonCount = (await _monService.GetAllNhomMonAsync()).Count;
				
				return new
				{
					success = true,
					message = "Database connection successful",
					monCount = monCount,
					nhomMonCount = nhomMonCount
				};
			}
			catch (Exception ex)
			{
				return new
				{
					success = false,
					message = "Database connection failed",
					error = ex.Message,
					stackTrace = ex.StackTrace
				};
			}
		}

		private object MapMonToFrontendProduct(MonResponseDto m, decimal? hotPercent)
		{
			decimal? percent = null;
			decimal? discountAmount = null;
			decimal basePrice = m.Gia;
			decimal finalPrice = basePrice;

			if (hotPercent.HasValue && hotPercent.Value > 0)
			{
				percent = hotPercent.Value;
				finalPrice = Math.Max(0, Math.Round(basePrice * (1 - percent.Value / 100m), 0, MidpointRounding.AwayFromZero));
				discountAmount = basePrice - finalPrice;
			}

			return new
			{
				idSanPham = m.MonId,
				IdSanPham = m.MonId,
				tenSp = m.Ten,
				TenSp = m.Ten,
				idDanhMuc = m.NhomMonId,
				IdDanhMuc = m.NhomMonId,
				tenDanhMuc = m.TenNhomMon,
				TenDanhMuc = m.TenNhomMon,
				gia = basePrice,
				Gia = basePrice,
				phanTramGiamGia = percent,
				PhanTramGiamGia = percent,
				soTienGiamGia = discountAmount,
				SoTienGiamGia = discountAmount,
				giaSauGiam = finalPrice,
				GiaSauGiam = finalPrice,
				tonKho = (int?)null,
				TonKho = (int?)null,
				hinh = m.HinhAnh,
				Hinh = m.HinhAnh,
				moTa = m.MoTaNgan,
				MoTa = m.MoTaNgan
			};
		}
	}
}


