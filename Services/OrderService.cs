using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuanLyThucAnNhanh.Data;
using QuanLyThucAnNhanh.DTOs;
using System.Data;
using System.Linq;

namespace QuanLyThucAnNhanh.Services
{
    public class OrderService
    {
        private readonly AppDbContext _db;

        public OrderService(AppDbContext db)
        {
            _db = db;
        }

        private static (string Key, string Label, string Description) NormalizeStatus(string? rawStatus)
        {
            if (string.IsNullOrWhiteSpace(rawStatus))
            {
                return ("processing", "Đang xử lý", "Đơn hàng đang được chuẩn bị để giao cho bạn");
            }

            var normalized = rawStatus.Replace(" ", string.Empty).ToLowerInvariant();

            return normalized switch
            {
                "choxuly" or "pending" => ("processing", "Đang xử lý", "Đơn hàng đang được chuẩn bị để giao cho bạn"),
                "danggiao" or "dangvanchuyen" or "shipping" => ("shipping", "Đang giao", "Đơn hàng đã rời cửa hàng và đang trên đường tới"),
                "dagiao" or "hoanthanh" or "completed" => ("completed", "Đã giao", "Đơn hàng đã giao thành công. Chúc bạn ngon miệng!"),
                "dahuy" or "cancelled" => ("cancelled", "Đã hủy", "Đơn hàng đã bị hủy. Liên hệ hỗ trợ nếu cần giúp đỡ"),
                _ => ("processing", "Đang xử lý", "Đơn hàng đang được chuẩn bị để giao cho bạn")
            };
        }

        private static string BuildOrderCode(long orderId, DateTime createdAt)
        {
            return $"DH-{createdAt:yyyyMMdd}-{orderId:D4}";
        }

        public async Task<IReadOnlyList<OrderSummaryDto>> GetOrdersForUserAsync(long userId, CancellationToken ct)
        {
            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync(ct);
            }

            const string sql = @"SELECT dh.DonHangId,
                                           dh.TaoLuc,
                                           dh.TrangThai,
                                           dh.TongTien,
                                           ISNULL(nd.HoTen, '') AS HoTen,
                                           ISNULL(nd.SoDienThoai, '') AS SoDienThoai,
                                           ISNULL((SELECT SUM(SoLuong) FROM dbo.DonHangChiTiet WHERE DonHangId = dh.DonHangId), 0) AS ItemCount
                                    FROM dbo.DonHang AS dh
                                    INNER JOIN dbo.NguoiDung AS nd ON nd.NguoiDungId = dh.NguoiDungId
                                    WHERE dh.NguoiDungId = @UserId
                                    ORDER BY dh.TaoLuc DESC";

            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add(new SqlParameter("@UserId", userId));

            var result = new List<OrderSummaryDto>();

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var orderId = reader.GetInt64(0);
                var createdAt = reader.GetDateTime(1);
                var rawStatus = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                var map = NormalizeStatus(rawStatus);

                var total = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3);
                var itemCount = reader.IsDBNull(6) ? 0 : Convert.ToInt32(reader.GetValue(6));

                result.Add(new OrderSummaryDto
                {
                    OrderId = orderId,
                    OrderCode = BuildOrderCode(orderId, createdAt),
                    CreatedAt = createdAt,
                    Status = map.Key,
                    StatusLabel = map.Label,
                    StatusDescription = map.Description,
                    Total = total,
                    PaymentMethod = "Thanh toán khi nhận hàng (COD)",
                    ItemCount = itemCount,
                    CustomerName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    CustomerPhone = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
                });
            }

            return result;
        }

        public async Task<OrderDetailDto?> GetOrderDetailAsync(long userId, long orderId, CancellationToken ct)
        {
            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync(ct);
            }

            const string orderSql = @"SELECT TOP 1 dh.DonHangId,
                                                   dh.TaoLuc,
                                                   dh.TrangThai,
                                                   dh.TongTien,
                                                   dh.ChiNhanhId,
                                                   ISNULL(nd.HoTen, '') AS HoTen,
                                                   ISNULL(nd.SoDienThoai, '') AS SoDienThoai
                                            FROM dbo.DonHang AS dh
                                            INNER JOIN dbo.NguoiDung AS nd ON nd.NguoiDungId = dh.NguoiDungId
                                            WHERE dh.DonHangId = @OrderId AND dh.NguoiDungId = @UserId";

            await using var orderCmd = new SqlCommand(orderSql, conn);
            orderCmd.Parameters.Add(new SqlParameter("@OrderId", orderId));
            orderCmd.Parameters.Add(new SqlParameter("@UserId", userId));

            await using var reader = await orderCmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
            {
                return null;
            }

            var createdAt = reader.GetDateTime(1);
            var rawStatus = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
            var map = NormalizeStatus(rawStatus);
            var total = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3);
            var branchId = reader.IsDBNull(4) ? (long?)null : reader.GetInt64(4);
            var customerName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
            var customerPhone = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);

            var detail = new OrderDetailDto
            {
                OrderId = orderId,
                OrderCode = BuildOrderCode(orderId, createdAt),
                CreatedAt = createdAt,
                Status = map.Key,
                StatusLabel = map.Label,
                StatusDescription = map.Description,
                Total = total,
                PaymentMethod = "Thanh toán khi nhận hàng (COD)",
                ItemCount = 0,
                CustomerName = customerName,
                CustomerPhone = customerPhone,
                Customer = new OrderCustomerDto
                {
                    Name = customerName,
                    Phone = customerPhone,
                    Address = branchId.HasValue ? $"Chi nhánh #{branchId.Value}" : string.Empty,
                    Note = string.Empty
                }
            };

            await reader.CloseAsync();

            const string itemsSql = @"SELECT c.MonId,
                                              ISNULL(m.Ten, '') AS Ten,
                                              c.SoLuong,
                                              c.DonGia,
                                              ISNULL(m.HinhAnh, '') AS HinhAnh
                                       FROM dbo.DonHangChiTiet AS c
                                       LEFT JOIN dbo.Mon AS m ON m.MonId = c.MonId
                                       WHERE c.DonHangId = @OrderId";

            await using var itemCmd = new SqlCommand(itemsSql, conn);
            itemCmd.Parameters.Add(new SqlParameter("@OrderId", orderId));

            await using var itemReader = await itemCmd.ExecuteReaderAsync(ct);
            decimal subtotal = 0m;
            while (await itemReader.ReadAsync(ct))
            {
                var price = itemReader.IsDBNull(3) ? 0m : itemReader.GetDecimal(3);
                var quantity = itemReader.IsDBNull(2) ? 0 : itemReader.GetInt32(2);
                subtotal += price * quantity;

                detail.Items.Add(new OrderItemDto
                {
                    MonId = itemReader.IsDBNull(0) ? 0 : itemReader.GetInt64(0),
                    Name = itemReader.IsDBNull(1) ? string.Empty : itemReader.GetString(1),
                    Quantity = quantity,
                    Price = price,
                    Image = itemReader.IsDBNull(4) ? string.Empty : itemReader.GetString(4)
                });
            }

            detail.ItemCount = detail.Items.Sum(x => x.Quantity);
            detail.Subtotal = subtotal;
            detail.ShippingFee = Math.Max(0, detail.Total - detail.Subtotal);
            detail.Discount = 0;

            if (detail.ShippingFee > detail.Total)
            {
                detail.ShippingFee = 0;
            }

            return detail;
        }

        public async Task<(IReadOnlyList<OrderSummaryDto> Orders, int Total)> GetAllOrdersAsync(
            string? statusFilter = null,
            string? paymentFilter = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int page = 1,
            int pageSize = 20,
            CancellationToken ct = default)
        {
            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync(ct);
            }

            var conditions = new List<string>();
            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "all")
            {
                var normalized = statusFilter.Replace(" ", string.Empty).ToLowerInvariant();
                var dbStatus = normalized switch
                {
                    "processing" => "ChoXuLy",
                    "shipping" => "DangGiao",
                    "completed" => "HoanThanh",
                    "cancelled" => "DaHuy",
                    _ => statusFilter
                };
                conditions.Add("dh.TrangThai = @StatusFilter");
                parameters.Add(new SqlParameter("@StatusFilter", dbStatus));
            }

            if (!string.IsNullOrWhiteSpace(paymentFilter) && paymentFilter != "all")
            {
                // Có thể mở rộng sau khi có bảng PaymentMethod
                // Hiện tại chỉ lọc COD hoặc tất cả
            }

            if (fromDate.HasValue)
            {
                conditions.Add("CAST(dh.TaoLuc AS DATE) >= @FromDate");
                parameters.Add(new SqlParameter("@FromDate", fromDate.Value.Date));
            }

            if (toDate.HasValue)
            {
                conditions.Add("CAST(dh.TaoLuc AS DATE) <= @ToDate");
                parameters.Add(new SqlParameter("@ToDate", toDate.Value.Date.AddDays(1).AddSeconds(-1)));
            }

            var whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

            // Count total
            var countSql = @"SELECT COUNT(*)
                              FROM dbo.DonHang AS dh
                              INNER JOIN dbo.NguoiDung AS nd ON nd.NguoiDungId = dh.NguoiDungId
                              " + whereClause;

            await using var countCmd = new SqlCommand(countSql, conn);
            foreach (var param in parameters)
            {
                countCmd.Parameters.Add(new SqlParameter(param.ParameterName, param.Value));
            }

            var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync(ct));

            // Get paginated data
            var offset = (page - 1) * pageSize;
            var dataSql = @"SELECT dh.DonHangId,
                                       dh.TaoLuc,
                                       dh.TrangThai,
                                       dh.TongTien,
                                       ISNULL(nd.HoTen, '') AS HoTen,
                                       ISNULL(nd.SoDienThoai, '') AS SoDienThoai,
                                       ISNULL((SELECT SUM(SoLuong) FROM dbo.DonHangChiTiet WHERE DonHangId = dh.DonHangId), 0) AS ItemCount
                                FROM dbo.DonHang AS dh
                                INNER JOIN dbo.NguoiDung AS nd ON nd.NguoiDungId = dh.NguoiDungId
                                " + whereClause + @"
                                ORDER BY dh.TaoLuc DESC
                                OFFSET @Offset ROWS
                                FETCH NEXT @PageSize ROWS ONLY";

            await using var dataCmd = new SqlCommand(dataSql, conn);
            foreach (var param in parameters)
            {
                dataCmd.Parameters.Add(new SqlParameter(param.ParameterName, param.Value));
            }
            dataCmd.Parameters.Add(new SqlParameter("@Offset", offset));
            dataCmd.Parameters.Add(new SqlParameter("@PageSize", pageSize));

            var result = new List<OrderSummaryDto>();

            await using var reader = await dataCmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var orderId = reader.GetInt64(0);
                var createdAt = reader.GetDateTime(1);
                var rawStatus = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                var map = NormalizeStatus(rawStatus);

                var totalAmount = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3);
                var itemCount = reader.IsDBNull(6) ? 0 : Convert.ToInt32(reader.GetValue(6));

                result.Add(new OrderSummaryDto
                {
                    OrderId = orderId,
                    OrderCode = BuildOrderCode(orderId, createdAt),
                    CreatedAt = createdAt,
                    Status = map.Key,
                    StatusLabel = map.Label,
                    StatusDescription = map.Description,
                    Total = totalAmount,
                    PaymentMethod = "Thanh toán khi nhận hàng (COD)",
                    ItemCount = itemCount,
                    CustomerName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    CustomerPhone = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
                });
            }

            return (result, total);
        }

        public async Task<OrderDetailDto?> GetOrderDetailForAdminAsync(long orderId, CancellationToken ct)
        {
            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync(ct);
            }

            const string orderSql = @"SELECT TOP 1 dh.DonHangId,
                                                   dh.TaoLuc,
                                                   dh.TrangThai,
                                                   dh.TongTien,
                                                   dh.ChiNhanhId,
                                                   ISNULL(nd.HoTen, '') AS HoTen,
                                                   ISNULL(nd.SoDienThoai, '') AS SoDienThoai
                                            FROM dbo.DonHang AS dh
                                            INNER JOIN dbo.NguoiDung AS nd ON nd.NguoiDungId = dh.NguoiDungId
                                            WHERE dh.DonHangId = @OrderId";

            await using var orderCmd = new SqlCommand(orderSql, conn);
            orderCmd.Parameters.Add(new SqlParameter("@OrderId", orderId));

            await using var reader = await orderCmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct))
            {
                return null;
            }

            var createdAt = reader.GetDateTime(1);
            var rawStatus = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
            var map = NormalizeStatus(rawStatus);
            var total = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3);
            var branchId = reader.IsDBNull(4) ? (long?)null : reader.GetInt64(4);
            var customerName = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
            var customerPhone = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);

            var detail = new OrderDetailDto
            {
                OrderId = orderId,
                OrderCode = BuildOrderCode(orderId, createdAt),
                CreatedAt = createdAt,
                Status = map.Key,
                StatusLabel = map.Label,
                StatusDescription = map.Description,
                Total = total,
                PaymentMethod = "Thanh toán khi nhận hàng (COD)",
                ItemCount = 0,
                CustomerName = customerName,
                CustomerPhone = customerPhone,
                Customer = new OrderCustomerDto
                {
                    Name = customerName,
                    Phone = customerPhone,
                    Address = branchId.HasValue ? $"Chi nhánh #{branchId.Value}" : string.Empty,
                    Note = string.Empty
                }
            };

            await reader.CloseAsync();

            const string itemsSql = @"SELECT c.MonId,
                                              ISNULL(m.Ten, '') AS Ten,
                                              c.SoLuong,
                                              c.DonGia,
                                              ISNULL(m.HinhAnh, '') AS HinhAnh
                                       FROM dbo.DonHangChiTiet AS c
                                       LEFT JOIN dbo.Mon AS m ON m.MonId = c.MonId
                                       WHERE c.DonHangId = @OrderId";

            await using var itemCmd = new SqlCommand(itemsSql, conn);
            itemCmd.Parameters.Add(new SqlParameter("@OrderId", orderId));

            await using var itemReader = await itemCmd.ExecuteReaderAsync(ct);
            decimal subtotal = 0m;
            while (await itemReader.ReadAsync(ct))
            {
                var price = itemReader.IsDBNull(3) ? 0m : itemReader.GetDecimal(3);
                var quantity = itemReader.IsDBNull(2) ? 0 : itemReader.GetInt32(2);
                subtotal += price * quantity;

                detail.Items.Add(new OrderItemDto
                {
                    MonId = itemReader.IsDBNull(0) ? 0 : itemReader.GetInt64(0),
                    Name = itemReader.IsDBNull(1) ? string.Empty : itemReader.GetString(1),
                    Quantity = quantity,
                    Price = price,
                    Image = itemReader.IsDBNull(4) ? string.Empty : itemReader.GetString(4)
                });
            }

            detail.ItemCount = detail.Items.Sum(x => x.Quantity);
            detail.Subtotal = subtotal;
            detail.ShippingFee = Math.Max(0, detail.Total - detail.Subtotal);
            detail.Discount = 0;

            if (detail.ShippingFee > detail.Total)
            {
                detail.ShippingFee = 0;
            }

            return detail;
        }

        public async Task<(long donHangId, decimal tongTien)> CreateOrderAsync(long userId, CreateOrderDto dto, CancellationToken ct)
        {
            if (dto.Items == null || dto.Items.Count == 0)
                throw new ArgumentException("Thiếu danh sách món");

            await using var conn = (SqlConnection)_db.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync(ct);

            await using var tx = await conn.BeginTransactionAsync(ct);
            try
            {
                decimal tongTien = 0;
                foreach (var it in dto.Items)
                {
                    await using var priceCmd = new SqlCommand("SELECT Gia FROM dbo.Mon WITH (NOLOCK) WHERE MonId=@MonId", conn, (SqlTransaction)tx);
                    priceCmd.Parameters.Add(new SqlParameter("@MonId", it.MonId));
                    var giaObj = await priceCmd.ExecuteScalarAsync(ct);
                    if (giaObj == null || giaObj is DBNull)
                        throw new InvalidOperationException($"MonId {it.MonId} không tồn tại");
                    var gia = Convert.ToDecimal(giaObj);
                    tongTien += gia * it.SoLuong;
                }

                long donHangId;
                await using (var cmd = new SqlCommand(@"INSERT INTO dbo.DonHang(NguoiDungId, ChiNhanhId, DiaChiId, TongTien, TrangThai, TaoLuc)
VALUES(@NguoiDungId, @ChiNhanhId, NULL, @TongTien, N'ChoXuLy', GETDATE());
SELECT CAST(SCOPE_IDENTITY() AS BIGINT);", conn, (SqlTransaction)tx))
                {
                    cmd.Parameters.Add(new SqlParameter("@NguoiDungId", dto.NguoiDungId));
                    cmd.Parameters.Add(new SqlParameter("@ChiNhanhId", dto.ChiNhanhId));
                    cmd.Parameters.Add(new SqlParameter("@TongTien", tongTien));
                    donHangId = (long)(await cmd.ExecuteScalarAsync(ct) ?? 0L);
                }

                foreach (var it in dto.Items)
                {
                    await using var giaCmd = new SqlCommand("SELECT Gia FROM dbo.Mon WITH (NOLOCK) WHERE MonId=@MonId", conn, (SqlTransaction)tx);
                    giaCmd.Parameters.Add(new SqlParameter("@MonId", it.MonId));
                    var gia = Convert.ToDecimal(await giaCmd.ExecuteScalarAsync(ct));

                    await using var ins = new SqlCommand(@"INSERT INTO dbo.DonHangChiTiet(DonHangId, MonId, SoLuong, DonGia)
VALUES(@DonHangId, @MonId, @SoLuong, @DonGia);", conn, (SqlTransaction)tx);
                    ins.Parameters.Add(new SqlParameter("@DonHangId", donHangId));
                    ins.Parameters.Add(new SqlParameter("@MonId", it.MonId));
                    ins.Parameters.Add(new SqlParameter("@SoLuong", it.SoLuong));
                    ins.Parameters.Add(new SqlParameter("@DonGia", gia));
                    await ins.ExecuteNonQueryAsync(ct);
                }

                await using (var call = new SqlCommand("dbo.sp_DeductInventoryForOrder", conn, (SqlTransaction)tx))
                {
                    call.CommandType = System.Data.CommandType.StoredProcedure;
                    call.Parameters.Add(new SqlParameter("@DonHangId", donHangId));
                    await call.ExecuteNonQueryAsync(ct);
                }

                await tx.CommitAsync(ct);
                return (donHangId, tongTien);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
        }
    }
}


