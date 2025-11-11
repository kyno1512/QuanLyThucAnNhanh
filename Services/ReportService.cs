using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuanLyThucAnNhanh.Data;
using QuanLyThucAnNhanh.DTOs;

namespace QuanLyThucAnNhanh.Services
{
    public class ReportService
    {
        private readonly AppDbContext _db;
        public ReportService(AppDbContext db) { _db = db; }

        public async Task<List<StockItemDto>> GetStockByBranchAsync(long chiNhanhId, CancellationToken ct)
        {
            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync(ct);
            var result = new List<StockItemDto>();
            var sql = @"SELECT t.NguyenLieuId, n.Ten, ISNULL(t.SoLuong,0) AS SoLuong
FROM dbo.TonKhoHienTai t
JOIN dbo.NguyenLieu n ON n.NguyenLieuId = t.NguyenLieuId
WHERE t.ChiNhanhId = @c
ORDER BY n.Ten";
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@c", chiNhanhId));
            await using var rd = await cmd.ExecuteReaderAsync(ct);
            while (await rd.ReadAsync(ct))
            {
                result.Add(new StockItemDto
                {
                    NguyenLieuId = rd.GetInt64(0),
                    Ten = rd.GetString(1),
                    SoLuong = rd.GetDecimal(2)
                });
            }
            return result;
        }
    }
}


