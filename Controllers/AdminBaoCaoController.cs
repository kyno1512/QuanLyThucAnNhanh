using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyThucAnNhanh.Services;

namespace QuanLyThucAnNhanh.Controllers
{
    [ApiController]
    [Route("api/admin/baocao")]
    [Authorize(Roles = "Admin")]
    public class AdminBaoCaoController : ControllerBase
    {
        private readonly ReportService _service;
        public AdminBaoCaoController(ReportService service) { _service = service; }

        [HttpGet("tonkho/{chiNhanhId}")]
        public async Task<IActionResult> TonKho(long chiNhanhId, CancellationToken ct)
        {
            var data = await _service.GetStockByBranchAsync(chiNhanhId, ct);
            return Ok(data);
        }
    }
}


