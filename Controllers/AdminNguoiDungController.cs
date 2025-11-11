using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyThucAnNhanh.DTOs;
using QuanLyThucAnNhanh.Services;

namespace QuanLyThucAnNhanh.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminNguoiDungController : ControllerBase
    {
        private readonly UserAdminService _service;
        public AdminNguoiDungController(UserAdminService service) { _service = service; }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateRole(long id, [FromBody] UpdateUserRoleDto dto)
        {
            var ok = await _service.UpdateRoleAsync(id, dto.VaiTro);
            return ok ? Ok() : NotFound();
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(long id, [FromBody] UpdateUserStatusDto dto)
        {
            var ok = await _service.UpdateStatusAsync(id, dto.TrangThai);
            return ok ? Ok() : NotFound();
        }

        [HttpPut("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(long id, [FromBody] ResetUserPasswordDto dto)
        {
            var ok = await _service.ResetPasswordAsync(id, dto.MatKhauMoi);
            return ok ? Ok() : NotFound();
        }
    }
}


