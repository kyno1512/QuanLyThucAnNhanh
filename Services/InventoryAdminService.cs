using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuanLyThucAnNhanh.Data;
using QuanLyThucAnNhanh.DTOs;

namespace QuanLyThucAnNhanh.Services
{
    public class InventoryAdminService
    {
        private readonly AppDbContext _db;
        public InventoryAdminService(AppDbContext db) { _db = db; }

        public async Task<long> CreatePhieuNhapAsync(CreatePhieuNhapDto dto, CancellationToken ct)
        {
            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            try
            {
                long phieuNhapId;
                await using (var cmd = new SqlCommand(@"INSERT INTO dbo.PhieuNhapKho(ChiNhanhId, NgayNhap, NguoiDungId)
VALUES(@ChiNhanhId, GETDATE(), @NguoiDungId); SELECT CAST(SCOPE_IDENTITY() AS BIGINT);", conn, (SqlTransaction)tx))
                {
                    cmd.Parameters.Add(new SqlParameter("@ChiNhanhId", dto.ChiNhanhId));
                    cmd.Parameters.Add(new SqlParameter("@NguoiDungId", (object?)dto.NguoiDungId ?? DBNull.Value));
                    phieuNhapId = (long)(await cmd.ExecuteScalarAsync(ct) ?? 0L);
                }

                foreach (var it in dto.Items)
                {
                    await using var ins = new SqlCommand(@"INSERT INTO dbo.ChiTietPhieuNhap(PhieuNhapId, NguyenLieuId, SoLuong, DonGia, HanSuDung)
VALUES(@PhieuNhapId, @NguyenLieuId, @SoLuong, @DonGia, @HanSuDung);", conn, (SqlTransaction)tx);
                    ins.Parameters.Add(new SqlParameter("@PhieuNhapId", phieuNhapId));
                    ins.Parameters.Add(new SqlParameter("@NguyenLieuId", it.NguyenLieuId));
                    ins.Parameters.Add(new SqlParameter("@SoLuong", it.SoLuong));
                    ins.Parameters.Add(new SqlParameter("@DonGia", (object?)it.DonGia ?? DBNull.Value));
                    ins.Parameters.Add(new SqlParameter("@HanSuDung", (object?)it.HanSuDung ?? DBNull.Value));
                    await ins.ExecuteNonQueryAsync(ct);

                    // Update TonKhoHienTai (+)
                    await UpsertTonKhoAsync(conn, tx, dto.ChiNhanhId, it.NguyenLieuId, it.SoLuong, ct);
                    await InsertGiaoDichAsync(conn, tx, it.NguyenLieuId, dto.ChiNhanhId, "Nhap", it.SoLuong, ct);
                }

                await tx.CommitAsync(ct);
                return phieuNhapId;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        public async Task<long> CreatePhieuXuatAsync(CreatePhieuXuatDto dto, CancellationToken ct)
        {
            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync(ct);
            await using var tx = await conn.BeginTransactionAsync(ct);

            try
            {
                // Check đủ tồn
                foreach (var it in dto.Items)
                {
                    var cur = await GetTonKhoAsync(conn, tx, dto.ChiNhanhId, it.NguyenLieuId, ct);
                    if (cur < it.SoLuong)
                        throw new InvalidOperationException($"Nguyên liệu {it.NguyenLieuId} không đủ tồn");
                }

                long phieuXuatId;
                await using (var cmd = new SqlCommand(@"INSERT INTO dbo.PhieuXuatKho(ChiNhanhId, NgayXuat, NguoiDungId, LyDo)
VALUES(@ChiNhanhId, GETDATE(), @NguoiDungId, @LyDo); SELECT CAST(SCOPE_IDENTITY() AS BIGINT);", conn, (SqlTransaction)tx))
                {
                    cmd.Parameters.Add(new SqlParameter("@ChiNhanhId", dto.ChiNhanhId));
                    cmd.Parameters.Add(new SqlParameter("@NguoiDungId", (object?)dto.NguoiDungId ?? DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@LyDo", (object?)dto.LyDo ?? DBNull.Value));
                    phieuXuatId = (long)(await cmd.ExecuteScalarAsync(ct) ?? 0L);
                }

                foreach (var it in dto.Items)
                {
                    await using var ins = new SqlCommand(@"INSERT INTO dbo.ChiTietPhieuXuat(PhieuXuatId, NguyenLieuId, SoLuong)
VALUES(@PhieuXuatId, @NguyenLieuId, @SoLuong);", conn, (SqlTransaction)tx);
                    ins.Parameters.Add(new SqlParameter("@PhieuXuatId", phieuXuatId));
                    ins.Parameters.Add(new SqlParameter("@NguyenLieuId", it.NguyenLieuId));
                    ins.Parameters.Add(new SqlParameter("@SoLuong", it.SoLuong));
                    await ins.ExecuteNonQueryAsync(ct);

                    // Update TonKhoHienTai (-)
                    await UpsertTonKhoAsync(conn, tx, dto.ChiNhanhId, it.NguyenLieuId, -it.SoLuong, ct);
                    await InsertGiaoDichAsync(conn, tx, it.NguyenLieuId, dto.ChiNhanhId, "Xuat", it.SoLuong, ct);
                }

                await tx.CommitAsync(ct);
                return phieuXuatId;
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }

        private static async Task<decimal> GetTonKhoAsync(SqlConnection conn, System.Data.Common.DbTransaction tx, long chiNhanhId, long nguyenLieuId, CancellationToken ct)
        {
            await using var cmd = new SqlCommand("SELECT ISNULL(SoLuong,0) FROM dbo.TonKhoHienTai WITH (NOLOCK) WHERE ChiNhanhId=@c AND NguyenLieuId=@n", conn, (SqlTransaction)tx);
            cmd.Parameters.Add(new SqlParameter("@c", chiNhanhId));
            cmd.Parameters.Add(new SqlParameter("@n", nguyenLieuId));
            var obj = await cmd.ExecuteScalarAsync(ct);
            return obj == null || obj is DBNull ? 0 : Convert.ToDecimal(obj);
        }

        private static async Task UpsertTonKhoAsync(SqlConnection conn, System.Data.Common.DbTransaction tx, long chiNhanhId, long nguyenLieuId, decimal delta, CancellationToken ct)
        {
            // Try update
            await using (var upd = new SqlCommand(@"UPDATE dbo.TonKhoHienTai
SET SoLuong = ISNULL(SoLuong,0) + @delta
WHERE ChiNhanhId=@c AND NguyenLieuId=@n; SELECT @@ROWCOUNT;", conn, (SqlTransaction)tx))
            {
                upd.Parameters.Add(new SqlParameter("@delta", delta));
                upd.Parameters.Add(new SqlParameter("@c", chiNhanhId));
                upd.Parameters.Add(new SqlParameter("@n", nguyenLieuId));
                var rows = Convert.ToInt32(await upd.ExecuteScalarAsync(ct));
                if (rows > 0) return;
            }
            // Insert new
            await using var ins = new SqlCommand(@"INSERT INTO dbo.TonKhoHienTai(NguyenLieuId, ChiNhanhId, SoLuong)
VALUES(@n, @c, @sl);", conn, (SqlTransaction)tx);
            ins.Parameters.Add(new SqlParameter("@n", nguyenLieuId));
            ins.Parameters.Add(new SqlParameter("@c", chiNhanhId));
            ins.Parameters.Add(new SqlParameter("@sl", Math.Max(0, delta))); // nhập mới thì delta dương
            await ins.ExecuteNonQueryAsync(ct);
        }

        private static async Task InsertGiaoDichAsync(SqlConnection conn, System.Data.Common.DbTransaction tx, long nguyenLieuId, long chiNhanhId, string loai, decimal soLuong, CancellationToken ct)
        {
            await using var ins = new SqlCommand("INSERT INTO dbo.GiaoDichKho(NguyenLieuId, ChiNhanhId, Loai, SoLuong, Ngay) VALUES(@n,@c,@l,@sl,GETDATE());", conn, (SqlTransaction)tx);
            ins.Parameters.Add(new SqlParameter("@n", nguyenLieuId));
            ins.Parameters.Add(new SqlParameter("@c", chiNhanhId));
            ins.Parameters.Add(new SqlParameter("@l", loai));
            ins.Parameters.Add(new SqlParameter("@sl", soLuong));
            await ins.ExecuteNonQueryAsync(ct);
        }
    }
}


