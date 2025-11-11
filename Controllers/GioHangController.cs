using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyThucAnNhanh.DTOs;
using QuanLyThucAnNhanh.Services;
using System.Security.Claims;

namespace QuanLyThucAnNhanh.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Yêu cầu đăng nhập
    public class GioHangController : ControllerBase
    {
        private readonly GioHangService _gioHangService;

        public GioHangController(GioHangService gioHangService)
        {
            _gioHangService = gioHangService;
        }

        // Helper method: Lấy UserId từ JWT token
        private long GetUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim == null || !long.TryParse(userIdClaim, out var userId))
                throw new UnauthorizedAccessException("Không thể lấy thông tin người dùng");

            return userId;
        }

        // 🔹 GET: api/giohang - Lấy giỏ hàng của user hiện tại
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                var userId = GetUserId();
                var cart = await _gioHangService.GetCartAsync(userId);
                
                if (cart == null)
                    return NotFound("Không tìm thấy giỏ hàng");
                
                return Ok(cart);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 🔹 POST: api/giohang - Thêm món vào giỏ hàng
        [HttpPost]
        public async Task<IActionResult> AddToCart(AddToCartDto dto)
        {
            try
            {
                var userId = GetUserId();
                var cart = await _gioHangService.AddToCartAsync(userId, dto);
                return Ok(cart);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 🔹 PUT: api/giohang/item/{id} - Cập nhật số lượng món trong giỏ
        [HttpPut("item/{gioHangChiTietId}")]
        public async Task<IActionResult> UpdateCartItem(long gioHangChiTietId, UpdateCartItemDto dto)
        {
            try
            {
                var userId = GetUserId();
                var cart = await _gioHangService.UpdateCartItemAsync(userId, gioHangChiTietId, dto);
                
                if (cart == null)
                    return NotFound("Không tìm thấy món trong giỏ hàng");
                
                return Ok(cart);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 🔹 DELETE: api/giohang/item/{id} - Xóa món khỏi giỏ hàng
        [HttpDelete("item/{gioHangChiTietId}")]
        public async Task<IActionResult> RemoveFromCart(long gioHangChiTietId)
        {
            try
            {
                var userId = GetUserId();
                var cart = await _gioHangService.RemoveFromCartAsync(userId, gioHangChiTietId);
                
                if (cart == null)
                    return NotFound("Không tìm thấy món trong giỏ hàng");
                
                return Ok(cart);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 🔹 DELETE: api/giohang - Xóa toàn bộ giỏ hàng
        [HttpDelete]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var userId = GetUserId();
                var result = await _gioHangService.ClearCartAsync(userId);
                
                if (!result)
                    return NotFound("Giỏ hàng trống");
                
                return Ok(new { message = "Đã xóa toàn bộ giỏ hàng" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 🔹 GET: api/giohang/check/{monId} - Kiểm tra món đã có trong giỏ chưa
        [HttpGet("check/{monId}")]
        public async Task<IActionResult> CheckItemInCart(long monId)
        {
            try
            {
                var userId = GetUserId();
                var isInCart = await _gioHangService.IsItemInCartAsync(userId, monId);
                
                return Ok(new { isInCart });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

