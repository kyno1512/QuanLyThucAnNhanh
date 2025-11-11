using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuanLyThucAnNhanh.DTOs;
using QuanLyThucAnNhanh.Services;

namespace QuanLyThucAnNhanh.Controllers
{
    [ApiController]
    [Route("api/admin/khuyenmai")]
    [Authorize(Roles = "Admin")]
    public class AdminKhuyenMaiController : ControllerBase
    {
        private readonly PromotionService _service;
        public AdminKhuyenMaiController(PromotionService service) { _service = service; }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpPost("hot-month/activate")]
        public async Task<IActionResult> ActivateHotMonth([FromBody] ActivateHotMonthDto dto)
        {
            if (dto == null || dto.AmountVnd <= 0) return BadRequest(new { message = "Số tiền giảm phải > 0" });
            var id = await _service.ActivateMonthlyHotDealAsync(dto.AmountVnd);
            return Ok(new { khuyenMaiId = id });
        }

        [HttpPost("hot-month/deactivate")]
        public async Task<IActionResult> DeactivateHotMonth()
        {
            var ok = await _service.DeactivateMonthlyHotDealAsync();
            return ok ? Ok() : NotFound();
        }

        [HttpGet("hot-month/products")]
        public async Task<IActionResult> GetHotMonthProducts()
        {
            var data = await _service.GetHotMonthProductsAsync();
            return Ok(data);
        }

        [HttpPost("hot-month/products")]
        public async Task<IActionResult> AddHotMonthProducts([FromBody] AddHotMonthProductsDto dto)
        {
            if (dto == null || dto.Products == null || dto.Products.Count == 0)
            {
                return BadRequest(new { message = "Danh sách sản phẩm trống" });
            }

            foreach (var item in dto.Products)
            {
                await _service.SetHotMonthProductPercentAsync(item.ProductId, item.Percent);
            }

            return Ok();
        }

        [HttpDelete("hot-month/products/{productId}")]
        public async Task<IActionResult> RemoveHotMonthProduct(long productId)
        {
            var ok = await _service.RemoveHotMonthProductAsync(productId);
            return ok ? Ok() : NotFound();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var item = await _service.GetByIdAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateKhuyenMaiDto dto)
        {
            var id = await _service.CreateAsync(dto);
            return Ok(new { khuyenMaiId = id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] CreateKhuyenMaiDto dto)
        {
            var ok = await _service.UpdateAsync(id, dto);
            return ok ? Ok() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            var ok = await _service.DeleteAsync(id);
            return ok ? Ok() : NotFound();
        }
    }

    public class ActivateHotMonthDto
    {
        public decimal AmountVnd { get; set; }
    }

    public class AddHotMonthProductsDto
    {
        public List<HotDealProductInput> Products { get; set; } = new();
    }

    public class HotDealProductInput
    {
        public long ProductId { get; set; }
        public decimal Percent { get; set; }
    }
}


