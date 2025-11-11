using Microsoft.AspNetCore.Mvc;
using QuanLyThucAnNhanh.DTOs;
using QuanLyThucAnNhanh.Services;
using Microsoft.AspNetCore.Authorization;

namespace QuanLyThucAnNhanh.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MonController : ControllerBase
    {
        private readonly MonService _monService;
        private readonly ReviewService _reviewService;
        private readonly InventoryService _inventoryService;

        public MonController(MonService monService, ReviewService reviewService, InventoryService inventoryService)
        {
            _monService = monService;
            _reviewService = reviewService;
            _inventoryService = inventoryService;
        }

        // 🔹 GET: api/mon - Lấy tất cả món
        [HttpGet]
        public async Task<IActionResult> GetAllMon()
        {
            var mons = await _monService.GetAllMonAsync();
            return Ok(mons);
        }

        // 🔹 GET: api/mon/{id} - Lấy món theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMonById(long id)
        {
            var mon = await _monService.GetMonByIdAsync(id);
            if (mon == null) return NotFound("Không tìm thấy món");
            return Ok(mon);
        }

        // 🔹 GET: api/mon/nhom/{nhomMonId} - Lấy món theo nhóm món
        [HttpGet("nhom/{nhomMonId}")]
        public async Task<IActionResult> GetMonByNhomMon(long nhomMonId)
        {
            var mons = await _monService.GetMonByNhomMonIdAsync(nhomMonId);
            return Ok(mons);
        }

        // ========== CHI TIẾT MÓN + ĐÁNH GIÁ ==========

        // 🔹 GET: api/mon/{id}/detail - Gọi sp_GetMonDetail
        [HttpGet("{id}/detail")]
        public async Task<IActionResult> GetMonDetail(long id)
        {
            var detail = await _reviewService.GetMonDetailAsync(id);
            if (detail == null) return NotFound("Không tìm thấy món");
            return Ok(detail);
        }

        // ========== KIỂM TRA TRƯỚC KHI ĐẶT HÀNG ==========

        // 🔹 GET: api/mon/{id}/available?chiNhanhId=1
        [HttpGet("{id}/available")]
        public async Task<IActionResult> GetAvailable(long id, [FromQuery] long chiNhanhId)
        {
            if (chiNhanhId <= 0) return BadRequest(new { message = "Thiếu chiNhanhId" });
            var available = await _inventoryService.GetAvailableAsync(id, chiNhanhId);
            return Ok(new { available });
        }

        // 🔹 GET: api/mon/{id}/capacity-today?chiNhanhId=1&ngay=2025-11-05
        [HttpGet("{id}/capacity-today")]
        public async Task<IActionResult> GetCapacityToday(long id, [FromQuery] long chiNhanhId, [FromQuery] DateTime ngay)
        {
            if (chiNhanhId <= 0) return BadRequest(new { message = "Thiếu chiNhanhId" });
            if (ngay == default) ngay = DateTime.UtcNow.Date;
            var capacityLeft = await _inventoryService.GetCapacityLeftTodayAsync(id, chiNhanhId, ngay);
            return Ok(new { capacityLeft });
        }

        // 🔹 GET: api/mon/{id}/ratings - Gọi sp_GetMonRatingsSummary
        [HttpGet("{id}/ratings")]
        public async Task<IActionResult> GetMonRatingsSummary(long id)
        {
            var summary = await _reviewService.GetMonRatingsSummaryAsync(id);
            return Ok(summary);
        }

        // 🔹 GET: api/mon/{id}/reviews?page=1&pageSize=10 - Gọi sp_GetMonReviews
        [HttpGet("{id}/reviews")]
        public async Task<IActionResult> GetMonReviews(long id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var reviews = await _reviewService.GetMonReviewsAsync(id, page, pageSize);
            return Ok(reviews);
        }

        // 🔹 POST: api/mon/{id}/reviews - Tạo đánh giá
        [HttpPost("{id}/reviews")]
        [Authorize]
        public async Task<IActionResult> CreateReview(long id, [FromBody] QuanLyThucAnNhanh.DTOs.CreateReviewDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { message = "Dữ liệu không hợp lệ", errors });
            }

            if (dto.Diem < 1 || dto.Diem > 5)
                return BadRequest(new { message = "Điểm phải từ 1 đến 5" });

            var newId = await _reviewService.CreateReviewAsync(id, dto);
            return Ok(new { DanhGiaMonId = newId });
        }

        // 🔹 POST: api/mon - Tạo món mới
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateMon([FromBody] CreateMonDto dto)
        {
            // Kiểm tra validation errors
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { message = "Dữ liệu không hợp lệ", errors });
            }

            try
            {
                var mon = await _monService.CreateMonAsync(dto);
                return CreatedAtAction(nameof(GetMonById), new { id = mon.MonId }, mon);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 🔹 PUT: api/mon/{id} - Cập nhật món
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateMon(long id, [FromBody] UpdateMonDto dto)
        {
            // Kiểm tra validation errors
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { message = "Dữ liệu không hợp lệ", errors });
            }

            try
            {
                var mon = await _monService.UpdateMonAsync(id, dto);
                if (mon == null) return NotFound(new { message = "Không tìm thấy món" });
                return Ok(mon);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 🔹 DELETE: api/mon/{id} - Xóa món (soft delete)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMon(long id)
        {
            var result = await _monService.DeleteMonAsync(id);
            if (!result) return NotFound("Không tìm thấy món");
            return Ok(new { message = "Xóa món thành công" });
        }

        // ========== API QUẢN LÝ NHÓM MÓN ==========

        // 🔹 GET: api/mon/nhommon - Lấy tất cả nhóm món
        [HttpGet("nhommon")]
        public async Task<IActionResult> GetAllNhomMon()
        {
            var nhomMons = await _monService.GetAllNhomMonAsync();
            return Ok(nhomMons);
        }

        // 🔹 GET: api/mon/nhommon/{id} - Lấy nhóm món theo ID
        [HttpGet("nhommon/{id}")]
        public async Task<IActionResult> GetNhomMonById(long id)
        {
            var nhomMon = await _monService.GetNhomMonByIdAsync(id);
            if (nhomMon == null) return NotFound("Không tìm thấy nhóm món");
            return Ok(nhomMon);
        }

        // 🔹 POST: api/mon/nhommon - Tạo nhóm món mới
        [HttpPost("nhommon")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateNhomMon(CreateNhomMonDto dto)
        {
            try
            {
                var nhomMon = await _monService.CreateNhomMonAsync(dto);
                return CreatedAtAction(nameof(GetNhomMonById), new { id = nhomMon.NhomMonId }, nhomMon);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 🔹 PUT: api/mon/nhommon/{id} - Cập nhật nhóm món
        [HttpPut("nhommon/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateNhomMon(long id, CreateNhomMonDto dto)
        {
            try
            {
                var nhomMon = await _monService.UpdateNhomMonAsync(id, dto);
                if (nhomMon == null) return NotFound("Không tìm thấy nhóm món");
                return Ok(nhomMon);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // 🔹 DELETE: api/mon/nhommon/{id} - Xóa nhóm món
        [HttpDelete("nhommon/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteNhomMon(long id)
        {
            try
            {
                var result = await _monService.DeleteNhomMonAsync(id);
                if (!result) return NotFound("Không tìm thấy nhóm món");
                return Ok(new { message = "Xóa nhóm món thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

