using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuanLyThucAnNhanh.Data;

namespace QuanLyThucAnNhanh.Services
{
    public class InventoryService
    {
        private readonly AppDbContext _db;

        public InventoryService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<int> GetAvailableAsync(long monId, long chiNhanhId, CancellationToken ct = default)
        {
            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync(ct);

            await using var cmd = new SqlCommand("dbo.sp_GetMonAvailableByBranch", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@MonId", monId));
            cmd.Parameters.Add(new SqlParameter("@ChiNhanhId", chiNhanhId));

            var obj = await cmd.ExecuteScalarAsync(ct);
            return obj == null || obj is DBNull ? 0 : Convert.ToInt32(obj);
        }

        public async Task<int> GetCapacityLeftTodayAsync(long monId, long chiNhanhId, DateTime ngay, CancellationToken ct = default)
        {
            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync(ct);

            await using var cmd = new SqlCommand("dbo.sp_GetMonCapacityLeftToday", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@MonId", monId));
            cmd.Parameters.Add(new SqlParameter("@ChiNhanhId", chiNhanhId));
            cmd.Parameters.Add(new SqlParameter("@Ngay", ngay.Date));

            var obj = await cmd.ExecuteScalarAsync(ct);
            return obj == null || obj is DBNull ? 0 : Convert.ToInt32(obj);
        }
    }
}


