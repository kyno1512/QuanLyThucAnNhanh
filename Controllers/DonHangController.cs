using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuanLyThucAnNhanh.Data;
using QuanLyThucAnNhanh.DTOs;
using QuanLyThucAnNhanh.Services;
using System.Security.Claims;

namespace QuanLyThucAnNhanh.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DonHangController : ControllerBase
    {
        private readonly OrderService _orders;

        public DonHangController(AppDbContext db)
        {
            _orders = new OrderService(db);
        }

        private long GetUserId()
        {
            var claimValue = User.FindFirst("UserId")?.Value;
            if (claimValue == null || !long.TryParse(claimValue, out var userId))
            {
                throw new UnauthorizedAccessException("Không thể lấy thông tin người dùng hiện tại");
            }

            return userId;
        }

        // GET: api/donhang
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMyOrders(CancellationToken ct)
        {
            try
            {
                var userId = GetUserId();
                var orders = await _orders.GetOrdersForUserAsync(userId, ct);
                return Ok(orders);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/donhang/{orderId}
        [HttpGet("{orderId:long}")]
        [Authorize]
        public async Task<IActionResult> GetOrderDetail(long orderId, CancellationToken ct)
        {
            try
            {
                var userId = GetUserId();
                var order = await _orders.GetOrderDetailAsync(userId, orderId, ct);
                if (order == null)
                {
                    return NotFound(new { message = "Không tìm thấy đơn hàng" });
                }

                return Ok(order);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/donhang/admin/all
        [HttpGet("admin/all")]
        [Authorize]
        public async Task<IActionResult> GetAllOrdersForAdmin(
            [FromQuery] string? status = null,
            [FromQuery] string? payment = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            try
            {
                // Kiểm tra quyền admin (có thể bổ sung sau)
                var (orders, total) = await _orders.GetAllOrdersAsync(
                    status, payment, fromDate, toDate, page, pageSize, ct);

                return Ok(new
                {
                    orders,
                    total,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/donhang/admin/{orderId}
        [HttpGet("admin/{orderId:long}")]
        [Authorize]
        public async Task<IActionResult> GetOrderDetailForAdmin(long orderId, CancellationToken ct)
        {
            try
            {
                var order = await _orders.GetOrderDetailForAdminAsync(orderId, ct);
                if (order == null)
                {
                    return NotFound(new { message = "Không tìm thấy đơn hàng" });
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/donhang
        [HttpPost]
        [Authorize] // yêu cầu đăng nhập (Khách hàng hoặc Admin đều được)
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto, CancellationToken ct)
        {
            try
            {
                var userId = GetUserId();
                // Sử dụng userId từ JWT token, không tin tưởng NguoiDungId từ DTO để tránh lỗ hổng bảo mật
                var (donHangId, tongTien) = await _orders.CreateOrderAsync(userId, dto, ct);
                return Ok(new { donHangId, tongTien, trangThai = "ChoXuLy" });
            }
            catch (SqlException ex)
            {
                if (ex.Message.Contains("Khong du ton kho", StringComparison.OrdinalIgnoreCase))
                    return StatusCode(409, new { message = "Không đủ tồn kho để xử lý đơn" });
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}


