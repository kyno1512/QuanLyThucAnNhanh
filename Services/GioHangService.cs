using Microsoft.EntityFrameworkCore;
using QuanLyThucAnNhanh.Data;
using QuanLyThucAnNhanh.DTOs;
using QuanLyThucAnNhanh.Models;

namespace QuanLyThucAnNhanh.Services
{
    public class GioHangService
    {
        private readonly AppDbContext _db;

        public GioHangService(AppDbContext db)
        {
            _db = db;
        }

        // 🔹 Lấy hoặc tạo giỏ hàng cho user
        private async Task<GioHang> GetOrCreateCartAsync(long nguoiDungId)
        {
            var gioHang = await _db.GioHangs
                .Include(g => g.ChiTiets!)
                    .ThenInclude(c => c.Mon)
                .FirstOrDefaultAsync(g => g.NguoiDungId == nguoiDungId);

            if (gioHang == null)
            {
                gioHang = new GioHang
                {
                    NguoiDungId = nguoiDungId,
                    TaoLuc = DateTime.Now
                };
                _db.GioHangs.Add(gioHang);
                await _db.SaveChangesAsync();

                // Load lại để có ChiTiets
                gioHang = await _db.GioHangs
                    .Include(g => g.ChiTiets!)
                        .ThenInclude(c => c.Mon)
                    .FirstOrDefaultAsync(g => g.GioHangId == gioHang.GioHangId);
            }

            return gioHang!;
        }

        // 🔹 Lấy giỏ hàng của user
        public async Task<CartResponseDto?> GetCartAsync(long nguoiDungId)
        {
            var gioHang = await GetOrCreateCartAsync(nguoiDungId);

            if (gioHang.ChiTiets == null || gioHang.ChiTiets.Count == 0)
            {
                return new CartResponseDto
                {
                    GioHangId = gioHang.GioHangId,
                    NguoiDungId = gioHang.NguoiDungId,
                    TaoLuc = gioHang.TaoLuc,
                    Items = new List<CartItemResponseDto>(),
                    TongTien = 0,
                    TongSoLuong = 0
                };
            }

            var items = gioHang.ChiTiets
                .Where(c => c.Mon != null && c.Mon.TrangThai == true)
                .Select(c => new CartItemResponseDto
                {
                    GioHangChiTietId = c.GioHangChiTietId,
                    MonId = c.MonId,
                    TenMon = c.Mon!.Ten,
                    GiaMon = c.Mon.Gia,
                    HinhAnhMon = c.Mon.HinhAnh,
                    SoLuong = c.SoLuong,
                    ThanhTien = c.Mon.Gia * c.SoLuong
                })
                .ToList();

            return new CartResponseDto
            {
                GioHangId = gioHang.GioHangId,
                NguoiDungId = gioHang.NguoiDungId,
                TaoLuc = gioHang.TaoLuc,
                Items = items,
                TongTien = items.Sum(i => i.ThanhTien),
                TongSoLuong = items.Sum(i => i.SoLuong)
            };
        }

        // 🔹 Thêm món vào giỏ hàng
        public async Task<CartResponseDto> AddToCartAsync(long nguoiDungId, AddToCartDto dto)
        {
            // Kiểm tra món có tồn tại và đang hoạt động không
            var mon = await _db.Mons.FirstOrDefaultAsync(m => m.MonId == dto.MonId && m.TrangThai == true);
            if (mon == null)
                throw new Exception("Món không tồn tại hoặc đã ngừng bán");

            // Lấy hoặc tạo giỏ hàng
            var gioHang = await GetOrCreateCartAsync(nguoiDungId);

            // Kiểm tra món đã có trong giỏ chưa
            var existingItem = await _db.GioHangChiTiets
                .FirstOrDefaultAsync(c => c.GioHangId == gioHang.GioHangId && c.MonId == dto.MonId);

            if (existingItem != null)
            {
                // Nếu đã có, cộng thêm số lượng
                existingItem.SoLuong += dto.SoLuong;
            }
            else
            {
                // Nếu chưa có, thêm mới
                var newItem = new GioHangChiTiet
                {
                    GioHangId = gioHang.GioHangId,
                    MonId = dto.MonId,
                    SoLuong = dto.SoLuong
                };
                _db.GioHangChiTiets.Add(newItem);
            }

            await _db.SaveChangesAsync();

            // Trả về giỏ hàng sau khi thêm
            return await GetCartAsync(nguoiDungId) ?? new CartResponseDto();
        }

        // 🔹 Cập nhật số lượng món trong giỏ
        public async Task<CartResponseDto?> UpdateCartItemAsync(long nguoiDungId, long gioHangChiTietId, UpdateCartItemDto dto)
        {
            // Kiểm tra item thuộc về user này
            var item = await _db.GioHangChiTiets
                .Include(c => c.GioHang)
                .FirstOrDefaultAsync(c => c.GioHangChiTietId == gioHangChiTietId 
                    && c.GioHang!.NguoiDungId == nguoiDungId);

            if (item == null)
                return null;

            item.SoLuong = dto.SoLuong;
            await _db.SaveChangesAsync();

            return await GetCartAsync(nguoiDungId);
        }

        // 🔹 Xóa món khỏi giỏ hàng
        public async Task<CartResponseDto?> RemoveFromCartAsync(long nguoiDungId, long gioHangChiTietId)
        {
            // Kiểm tra item thuộc về user này
            var item = await _db.GioHangChiTiets
                .Include(c => c.GioHang)
                .FirstOrDefaultAsync(c => c.GioHangChiTietId == gioHangChiTietId 
                    && c.GioHang!.NguoiDungId == nguoiDungId);

            if (item == null)
                return null;

            _db.GioHangChiTiets.Remove(item);
            await _db.SaveChangesAsync();

            return await GetCartAsync(nguoiDungId);
        }

        // 🔹 Xóa toàn bộ giỏ hàng
        public async Task<bool> ClearCartAsync(long nguoiDungId)
        {
            var gioHang = await _db.GioHangs
                .Include(g => g.ChiTiets)
                .FirstOrDefaultAsync(g => g.NguoiDungId == nguoiDungId);

            if (gioHang == null || gioHang.ChiTiets == null || gioHang.ChiTiets.Count == 0)
                return false;

            _db.GioHangChiTiets.RemoveRange(gioHang.ChiTiets);
            await _db.SaveChangesAsync();

            return true;
        }

        // 🔹 Kiểm tra món đã có trong giỏ chưa
        public async Task<bool> IsItemInCartAsync(long nguoiDungId, long monId)
        {
            var gioHang = await _db.GioHangs
                .FirstOrDefaultAsync(g => g.NguoiDungId == nguoiDungId);

            if (gioHang == null)
                return false;

            return await _db.GioHangChiTiets
                .AnyAsync(c => c.GioHangId == gioHang.GioHangId && c.MonId == monId);
        }
    }
}

