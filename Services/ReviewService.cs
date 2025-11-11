using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuanLyThucAnNhanh.Data;
using QuanLyThucAnNhanh.DTOs;
using QuanLyThucAnNhanh.Models;

namespace QuanLyThucAnNhanh.Services
{
    public class ReviewService
    {
        private readonly AppDbContext _db;

        public ReviewService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<MonDetailDto?> GetMonDetailAsync(long monId, CancellationToken ct = default)
        {
            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            await EnsureOpenAsync(conn, ct);

            await using var cmd = new SqlCommand("dbo.sp_GetMonDetail", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@MonId", monId));

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct)) return null;

            return new MonDetailDto
            {
                MonId = reader.GetInt64(reader.GetOrdinal("MonId")),
                NhomMonId = reader.GetInt64(reader.GetOrdinal("NhomMonId")),
                Ten = reader.GetString(reader.GetOrdinal("Ten")),
                Gia = reader.GetDecimal(reader.GetOrdinal("Gia")),
                MoTaNgan = reader.IsDBNull(reader.GetOrdinal("MoTaNgan")) ? null : reader.GetString(reader.GetOrdinal("MoTaNgan")),
                HinhAnhUrl = reader.IsDBNull(reader.GetOrdinal("HinhAnhUrl")) ? null : reader.GetString(reader.GetOrdinal("HinhAnhUrl"))
            };
        }

        public async Task<RatingsSummaryDto> GetMonRatingsSummaryAsync(long monId, CancellationToken ct = default)
        {
            var result = new RatingsSummaryDto();

            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            await EnsureOpenAsync(conn, ct);

            await using var cmd = new SqlCommand("dbo.sp_GetMonRatingsSummary", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@MonId", monId));

            await using var reader = await cmd.ExecuteReaderAsync(ct);

            if (await reader.ReadAsync(ct))
            {
                result.AvgRating = reader.IsDBNull(reader.GetOrdinal("AvgRating"))
                    ? null
                    : reader.GetDecimal(reader.GetOrdinal("AvgRating"));
                result.TotalReviews = reader.IsDBNull(reader.GetOrdinal("TotalReviews"))
                    ? 0
                    : reader.GetInt32(reader.GetOrdinal("TotalReviews"));
            }

            if (await reader.NextResultAsync(ct))
            {
                while (await reader.ReadAsync(ct))
                {
                    result.Distribution.Add(new StarCountDto
                    {
                        Sao = reader.GetInt32(reader.GetOrdinal("sao")),
                        CountPerStar = reader.IsDBNull(reader.GetOrdinal("CountPerStar"))
                            ? 0
                            : reader.GetInt32(reader.GetOrdinal("CountPerStar"))
                    });
                }
            }

            if (result.Distribution.Count == 0)
            {
                for (var s = 5; s >= 1; s--)
                {
                    result.Distribution.Add(new StarCountDto { Sao = s, CountPerStar = 0 });
                }
            }

            return result;
        }

        public async Task<PagedReviewsDto> GetMonReviewsAsync(long monId, int page, int pageSize, CancellationToken ct = default)
        {
            var pageFixed = page <= 0 ? 1 : page;
            var sizeFixed = pageSize <= 0 ? 10 : pageSize;
            var result = new PagedReviewsDto { Page = pageFixed, PageSize = sizeFixed };

            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            await EnsureOpenAsync(conn, ct);

            await using var cmd = new SqlCommand("dbo.sp_GetMonReviews", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.Add(new SqlParameter("@MonId", monId));
            cmd.Parameters.Add(new SqlParameter("@Page", pageFixed));
            cmd.Parameters.Add(new SqlParameter("@PageSize", sizeFixed));

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                result.Items.Add(new ReviewDto
                {
                    DanhGiaMonId = reader.GetInt64(reader.GetOrdinal("DanhGiaMonId")),
                    NguoiDungId = reader.GetInt64(reader.GetOrdinal("NguoiDungId")),
                    Diem = reader.GetInt32(reader.GetOrdinal("diem")),
                    NhanXet = reader.IsDBNull(reader.GetOrdinal("NhanXet")) ? null : reader.GetString(reader.GetOrdinal("NhanXet")),
                    Ngay = reader.GetDateTime(reader.GetOrdinal("Ngay"))
                });
            }

            return result;
        }

        public async Task<long> CreateReviewAsync(long monId, CreateReviewDto dto, CancellationToken ct = default)
        {
            var review = new DanhGiaMon
            {
                MonId = monId,
                NguoiDungId = dto.NguoiDungId,
                Diem = dto.Diem,
                NhanXet = dto.NhanXet,
                Ngay = DateTime.UtcNow
            };

            _db.DanhGiaMons.Add(review);
            await _db.SaveChangesAsync(ct);
            return review.DanhGiaMonId;
        }

        private static async Task EnsureOpenAsync(SqlConnection conn, CancellationToken ct)
        {
            if (conn.State != System.Data.ConnectionState.Open)
            {
                await conn.OpenAsync(ct);
            }
        }
    }
}


