using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyThucAnNhanh.DTOs;
using QuanLyThucAnNhanh.Services;

namespace QuanLyThucAnNhanh.Controllers
{
    [ApiController]
    [Route("api/admin/kho")]
    [Authorize(Roles = "Admin")]
    public class AdminKhoController : ControllerBase
    {
        private readonly InventoryAdminService _service;
        public AdminKhoController(InventoryAdminService service) { _service = service; }

        [HttpPost("nhap")]
        public async Task<IActionResult> NhapKho([FromBody] CreatePhieuNhapDto dto, CancellationToken ct)
        {
            if (dto.Items == null || dto.Items.Count == 0) return BadRequest(new { message = "Thiếu danh sách nhập" });
            var id = await _service.CreatePhieuNhapAsync(dto, ct);
            return Ok(new { phieuNhapId = id });
        }

        [HttpPost("xuat")]
        public async Task<IActionResult> XuatKho([FromBody] CreatePhieuXuatDto dto, CancellationToken ct)
        {
            if (dto.Items == null || dto.Items.Count == 0) return BadRequest(new { message = "Thiếu danh sách xuất" });
            try
            {
                var id = await _service.CreatePhieuXuatAsync(dto, ct);
                return Ok(new { phieuXuatId = id });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(409, new { message = ex.Message });
            }
        }
    }
}


