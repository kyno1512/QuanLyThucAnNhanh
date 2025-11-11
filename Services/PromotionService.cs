using Microsoft.EntityFrameworkCore;
using QuanLyThucAnNhanh.Data;
using QuanLyThucAnNhanh.DTOs;

namespace QuanLyThucAnNhanh.Services
{
    public class PromotionService
    {
        private const string HotMonthCode = "HOT_MONTH";
        private const string HotMonthProductPrefix = "HOT_MONTH_PRODUCT_";

        private readonly AppDbContext _db;
        public PromotionService(AppDbContext db) { _db = db; }

        public async Task<List<KhuyenMaiDto>> GetAllAsync()
        {
            return await _db.Set<Models.KhuyenMai>()
                .Select(k => new KhuyenMaiDto
                {
                    KhuyenMaiId = k.KhuyenMaiId,
                    Ma = k.Ma,
                    MoTa = k.MoTa,
                    Loai = k.Loai,
                    GiaTri = k.GiaTri,
                    BatDau = k.BatDau,
                    KetThuc = k.KetThuc,
                    SoLuongToiDa = k.SoLuongToiDa,
                    TrangThai = k.TrangThai
                }).ToListAsync();
        }

        public async Task<KhuyenMaiDto?> GetByIdAsync(long id)
        {
            var k = await _db.Set<Models.KhuyenMai>().FindAsync(id);
            if (k == null) return null;
            return new KhuyenMaiDto
            {
                KhuyenMaiId = k.KhuyenMaiId,
                Ma = k.Ma,
                MoTa = k.MoTa,
                Loai = k.Loai,
                GiaTri = k.GiaTri,
                BatDau = k.BatDau,
                KetThuc = k.KetThuc,
                SoLuongToiDa = k.SoLuongToiDa,
                TrangThai = k.TrangThai
            };
        }

        public async Task<long> CreateAsync(CreateKhuyenMaiDto dto)
        {
            var km = new Models.KhuyenMai
            {
                Ma = dto.Ma,
                MoTa = dto.MoTa,
                Loai = dto.Loai,
                GiaTri = dto.GiaTri,
                BatDau = dto.BatDau,
                KetThuc = dto.KetThuc,
                SoLuongToiDa = dto.SoLuongToiDa,
                TrangThai = dto.TrangThai ?? true
            };
            _db.Add(km);
            await _db.SaveChangesAsync();
            return km.KhuyenMaiId;
        }

        public async Task<bool> UpdateAsync(long id, CreateKhuyenMaiDto dto)
        {
            var km = await _db.Set<Models.KhuyenMai>().FindAsync(id);
            if (km == null) return false;
            km.Ma = dto.Ma;
            km.MoTa = dto.MoTa;
            km.Loai = dto.Loai;
            km.GiaTri = dto.GiaTri;
            km.BatDau = dto.BatDau;
            km.KetThuc = dto.KetThuc;
            km.SoLuongToiDa = dto.SoLuongToiDa;
            km.TrangThai = dto.TrangThai ?? km.TrangThai;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            var km = await _db.Set<Models.KhuyenMai>().FindAsync(id);
            if (km == null) return false;
            _db.Remove(km);
            await _db.SaveChangesAsync();
            return true;
        }

        // --------- HOT DEAL THÁNG NÀY ----------
        // HOT_MONTH: giảm amount chung toàn cửa hàng
        // HOT_MONTH_PRODUCT_{id}: giảm phần trăm cho từng sản phẩm
        public async Task<long> ActivateMonthlyHotDealAsync(decimal amountVnd)
        {
            var (start, end) = GetCurrentMonthRangeUtc();

            var set = _db.Set<Models.KhuyenMai>();
            var exist = await set.FirstOrDefaultAsync(x => x.Ma == HotMonthCode);
            if (exist == null)
            {
                exist = new Models.KhuyenMai
                {
                    Ma = HotMonthCode,
                    MoTa = "Khuyến mãi sốc tháng hiện tại (amount)",
                    Loai = "amount",
                    GiaTri = amountVnd,
                    BatDau = start,
                    KetThuc = end,
                    TrangThai = true
                };
                set.Add(exist);
                await _db.SaveChangesAsync();
                return exist.KhuyenMaiId;
            }

            exist.Loai = "amount";
            exist.GiaTri = amountVnd;
            exist.BatDau = start;
            exist.KetThuc = end;
            exist.TrangThai = true;
            await _db.SaveChangesAsync();
            return exist.KhuyenMaiId;
        }

        public async Task<bool> DeactivateMonthlyHotDealAsync()
        {
            var set = _db.Set<Models.KhuyenMai>();
            var exist = await set.FirstOrDefaultAsync(x => x.Ma == HotMonthCode);
            if (exist == null) return false;
            exist.TrangThai = false;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<List<HotDealProductDto>> GetHotMonthProductsAsync()
        {
            var (start, end) = GetCurrentMonthRangeUtc();
            var prefix = HotMonthProductPrefix;
            var now = DateTime.UtcNow;

            var list = await _db.Set<Models.KhuyenMai>()
                .Where(x =>
                    x.Ma.StartsWith(prefix) &&
                    x.TrangThai == true &&
                    x.Loai == "percent" &&
                    x.GiaTri.HasValue &&
                    x.GiaTri.Value > 0 &&
                    x.BatDau <= end &&
                    x.KetThuc >= start)
                .Select(x => new { x.Ma, x.GiaTri })
                .ToListAsync();

            var result = new List<HotDealProductDto>();
            foreach (var item in list)
            {
                if (long.TryParse(item.Ma.Substring(prefix.Length), out var productId))
                {
                    result.Add(new HotDealProductDto
                    {
                        ProductId = productId,
                        Percent = item.GiaTri!.Value
                    });
                }
            }
            return result;
        }

        public async Task SetHotMonthProductPercentAsync(long productId, decimal percent)
        {
            if (productId <= 0) throw new ArgumentException("Invalid productId");
            if (percent <= 0 || percent > 100) throw new ArgumentException("Percent must be in (0,100]");

            var (start, end) = GetCurrentMonthRangeUtc();
            var code = HotMonthProductPrefix + productId;
            var set = _db.Set<Models.KhuyenMai>();
            var exist = await set.FirstOrDefaultAsync(x => x.Ma == code);
            if (exist == null)
            {
                var km = new Models.KhuyenMai
                {
                    Ma = code,
                    MoTa = $"Hot deal tháng cho sản phẩm {productId}",
                    Loai = "percent",
                    GiaTri = percent,
                    BatDau = start,
                    KetThuc = end,
                    TrangThai = true
                };
                set.Add(km);
            }
            else
            {
                exist.Loai = "percent";
                exist.GiaTri = percent;
                exist.BatDau = start;
                exist.KetThuc = end;
                exist.TrangThai = true;
            }
            await _db.SaveChangesAsync();
        }

        public async Task<bool> RemoveHotMonthProductAsync(long productId)
        {
            if (productId <= 0) return false;
            var code = HotMonthProductPrefix + productId;
            var set = _db.Set<Models.KhuyenMai>();
            var exist = await set.FirstOrDefaultAsync(x => x.Ma == code);
            if (exist == null) return false;
            exist.TrangThai = false;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<Dictionary<long, decimal>> GetHotDealPercentLookupAsync(IEnumerable<long> productIds)
        {
            var ids = productIds?.Distinct().ToList() ?? new List<long>();
            if (ids.Count == 0) return new Dictionary<long, decimal>();

            var (start, end) = GetCurrentMonthRangeUtc();
            var codes = ids.Select(id => HotMonthProductPrefix + id).ToList();

            var list = await _db.Set<Models.KhuyenMai>()
                .Where(x =>
                    codes.Contains(x.Ma) &&
                    x.TrangThai == true &&
                    x.Loai == "percent" &&
                    x.GiaTri.HasValue &&
                    x.GiaTri.Value > 0 &&
                    x.BatDau <= end &&
                    x.KetThuc >= start)
                .Select(x => new { x.Ma, x.GiaTri })
                .ToListAsync();

            var dict = new Dictionary<long, decimal>();
            foreach (var item in list)
            {
                if (long.TryParse(item.Ma.Substring(HotMonthProductPrefix.Length), out var productId))
                {
                    var percent = item.GiaTri ?? 0;
                    if (percent > 0) dict[productId] = percent;
                }
            }
            return dict;
        }

        private static (DateTime start, DateTime end) GetCurrentMonthRangeUtc()
        {
            var now = DateTime.UtcNow;
            var start = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1).AddSeconds(-1);
            return (start, end);
        }
    }

    public class HotDealProductDto
    {
        public long ProductId { get; set; }
        public decimal Percent { get; set; }
    }
}


