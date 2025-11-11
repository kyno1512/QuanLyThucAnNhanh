using Microsoft.EntityFrameworkCore;
using QuanLyThucAnNhanh.Data;
using QuanLyThucAnNhanh.DTOs;
using QuanLyThucAnNhanh.Models;

namespace QuanLyThucAnNhanh.Services
{
    public class MonService
    {
        private readonly AppDbContext _db;

        public MonService(AppDbContext db)
        {
            _db = db;
        }

        // 🔹 Lấy tất cả món
        public async Task<List<MonResponseDto>> GetAllMonAsync()
        {
            try
            {
                // Kiểm tra database có kết nối được không
                if (!await _db.Database.CanConnectAsync())
                {
                    throw new Exception("Không thể kết nối đến database");
                }

                var mons = await _db.Mons
                    .Include(m => m.NhomMon)
                    .Where(m => m.TrangThai == true)
                    .ToListAsync();
                
                return mons.Select(m => new MonResponseDto
                {
                    MonId = m.MonId,
                    NhomMonId = m.NhomMonId,
                    TenNhomMon = m.NhomMon?.Ten ?? string.Empty,
                    Ten = m.Ten ?? string.Empty,
                    Gia = m.Gia,
                    TrangThai = m.TrangThai,
                    HinhAnh = m.HinhAnh,
                    MoTaNgan = m.MoTaNgan
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách món: {ex.Message}", ex);
            }
        }

        // 🔹 Lấy món theo ID
        public async Task<MonResponseDto?> GetMonByIdAsync(long monId)
        {
            var mon = await _db.Mons
                .Include(m => m.NhomMon)
                .FirstOrDefaultAsync(m => m.MonId == monId);

            if (mon == null) return null;

            return new MonResponseDto
            {
                MonId = mon.MonId,
                NhomMonId = mon.NhomMonId,
                TenNhomMon = mon.NhomMon?.Ten ?? "",
                Ten = mon.Ten,
                Gia = mon.Gia,
                TrangThai = mon.TrangThai,
                HinhAnh = mon.HinhAnh,
                MoTaNgan = mon.MoTaNgan
            };
        }

        // 🔹 Lấy món theo nhóm món
        public async Task<List<MonResponseDto>> GetMonByNhomMonIdAsync(long nhomMonId)
        {
            return await _db.Mons
                .Include(m => m.NhomMon)
                .Where(m => m.NhomMonId == nhomMonId && m.TrangThai == true)
                .Select(m => new MonResponseDto
                {
                    MonId = m.MonId,
                    NhomMonId = m.NhomMonId,
                    TenNhomMon = m.NhomMon != null ? m.NhomMon.Ten : string.Empty,
                    Ten = m.Ten,
                    Gia = m.Gia,
                    TrangThai = m.TrangThai,
                    HinhAnh = m.HinhAnh,
                    MoTaNgan = m.MoTaNgan
                })
                .ToListAsync();
        }

        // 🔹 Tạo món mới
        public async Task<MonResponseDto> CreateMonAsync(CreateMonDto dto)
        {
            // Kiểm tra nhóm món có tồn tại không
            var nhomMon = await _db.NhomMons.FindAsync(dto.NhomMonId);
            if (nhomMon == null)
                throw new Exception("Nhóm món không tồn tại");

            var mon = new Mon
            {
                NhomMonId = dto.NhomMonId,
                Ten = dto.Ten,
                Gia = dto.Gia,
                TrangThai = dto.TrangThai,
                HinhAnh = dto.HinhAnh,
                MoTaNgan = dto.MoTaNgan
            };

            _db.Mons.Add(mon);
            await _db.SaveChangesAsync();

            return new MonResponseDto
            {
                MonId = mon.MonId,
                NhomMonId = mon.NhomMonId,
                TenNhomMon = nhomMon.Ten,
                Ten = mon.Ten,
                Gia = mon.Gia,
                TrangThai = mon.TrangThai,
                HinhAnh = mon.HinhAnh,
                MoTaNgan = mon.MoTaNgan
            };
        }

        // 🔹 Cập nhật món
        public async Task<MonResponseDto?> UpdateMonAsync(long monId, UpdateMonDto dto)
        {
            var mon = await _db.Mons.FindAsync(monId);
            if (mon == null) return null;

            // Kiểm tra nhóm món có tồn tại không
            var nhomMon = await _db.NhomMons.FindAsync(dto.NhomMonId);
            if (nhomMon == null)
                throw new Exception("Nhóm món không tồn tại");

            mon.NhomMonId = dto.NhomMonId;
            mon.Ten = dto.Ten;
            mon.Gia = dto.Gia;
            mon.TrangThai = dto.TrangThai;
            mon.HinhAnh = dto.HinhAnh;
            mon.MoTaNgan = dto.MoTaNgan;

            await _db.SaveChangesAsync();

            return new MonResponseDto
            {
                MonId = mon.MonId,
                NhomMonId = mon.NhomMonId,
                TenNhomMon = nhomMon.Ten,
                Ten = mon.Ten,
                Gia = mon.Gia,
                TrangThai = mon.TrangThai,
                HinhAnh = mon.HinhAnh,
                MoTaNgan = mon.MoTaNgan
            };
        }

        // 🔹 Xóa món (soft delete - chỉ đổi TrangThai)
        public async Task<bool> DeleteMonAsync(long monId)
        {
            var mon = await _db.Mons.FindAsync(monId);
            if (mon == null) return false;

            mon.TrangThai = false;
            await _db.SaveChangesAsync();
            return true;
        }

        // ========== QUẢN LÝ NHÓM MÓN ==========

        // 🔹 Lấy tất cả nhóm món
        public async Task<List<NhomMonResponseDto>> GetAllNhomMonAsync()
        {
            try
            {
                // Kiểm tra database có kết nối được không
                if (!await _db.Database.CanConnectAsync())
                {
                    throw new Exception("Không thể kết nối đến database");
                }

                var nhomMons = await _db.NhomMons.ToListAsync();
                return nhomMons.Select(n => new NhomMonResponseDto
                {
                    NhomMonId = n.NhomMonId,
                    Ten = n.Ten ?? string.Empty,
                    SoLuongMon = 0 // Tính sau để tránh vòng lặp
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách nhóm món: {ex.Message}", ex);
            }
        }

        // 🔹 Lấy nhóm món theo ID
        public async Task<NhomMonResponseDto?> GetNhomMonByIdAsync(long nhomMonId)
        {
            var nhomMon = await _db.NhomMons
                .Include(n => n.Mons)
                .FirstOrDefaultAsync(n => n.NhomMonId == nhomMonId);

            if (nhomMon == null) return null;

            return new NhomMonResponseDto
            {
                NhomMonId = nhomMon.NhomMonId,
                Ten = nhomMon.Ten,
                SoLuongMon = nhomMon.Mons != null ? nhomMon.Mons.Count(m => m.TrangThai == true) : 0
            };
        }

        // 🔹 Tạo nhóm món mới
        public async Task<NhomMonResponseDto> CreateNhomMonAsync(CreateNhomMonDto dto)
        {
            var nhomMon = new NhomMon
            {
                Ten = dto.Ten
            };

            _db.NhomMons.Add(nhomMon);
            await _db.SaveChangesAsync();

            return new NhomMonResponseDto
            {
                NhomMonId = nhomMon.NhomMonId,
                Ten = nhomMon.Ten,
                SoLuongMon = 0
            };
        }

        // 🔹 Cập nhật nhóm món
        public async Task<NhomMonResponseDto?> UpdateNhomMonAsync(long nhomMonId, CreateNhomMonDto dto)
        {
            var nhomMon = await _db.NhomMons
                .Include(n => n.Mons)
                .FirstOrDefaultAsync(n => n.NhomMonId == nhomMonId);

            if (nhomMon == null) return null;

            nhomMon.Ten = dto.Ten;
            await _db.SaveChangesAsync();

            return new NhomMonResponseDto
            {
                NhomMonId = nhomMon.NhomMonId,
                Ten = nhomMon.Ten,
                SoLuongMon = nhomMon.Mons != null ? nhomMon.Mons.Count(m => m.TrangThai == true) : 0
            };
        }

        // 🔹 Xóa nhóm món (chỉ khi không có món nào)
        public async Task<bool> DeleteNhomMonAsync(long nhomMonId)
        {
            var nhomMon = await _db.NhomMons
                .Include(n => n.Mons)
                .FirstOrDefaultAsync(n => n.NhomMonId == nhomMonId);

            if (nhomMon == null) return false;

            // Kiểm tra có món nào đang dùng nhóm này không
            if (nhomMon.Mons != null && nhomMon.Mons.Any(m => m.TrangThai == true))
                throw new Exception("Không thể xóa nhóm món đang có món đang hoạt động");

            _db.NhomMons.Remove(nhomMon);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}

