using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using QuanLyThucAnNhanh.Services;
using QuanLyThucAnNhanh.DTOs;

namespace QuanLyThucAnNhanh.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductController(ProductService productService)
        {
            _productService = productService;
        }

        // GET: api/Product/list?page=1&pageSize=1000
        [HttpGet("list")]
        public async Task<IActionResult> GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 100, 
            [FromQuery] string? tenSp = null, [FromQuery] long? idDanhMuc = null,
            [FromQuery] decimal? giaMin = null, [FromQuery] decimal? giaMax = null)
        {
            try
            {
                Console.WriteLine($"GetProducts called: page={page}, pageSize={pageSize}");
                var result = await _productService.GetProductsAsync(new ProductListQuery
                {
                    Page = page,
                    PageSize = pageSize,
                    TenSp = tenSp,
                    IdDanhMuc = idDanhMuc,
                    GiaMin = giaMin,
                    GiaMax = giaMax
                });
                Console.WriteLine("GetProducts success");
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log full exception details
                var errorDetails = new
                {
                    message = "Lỗi khi lấy danh sách sản phẩm",
                    error = ex.Message,
                    stackTrace = ex.StackTrace,
                    innerException = ex.InnerException?.Message,
                    innerStackTrace = ex.InnerException?.StackTrace
                };
                Console.WriteLine($"ERROR GetProducts: {ex.GetType().Name} - {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner Stack: {ex.InnerException.StackTrace}");
                }
                return StatusCode(500, errorDetails);
            }
        }

        // Test endpoint để kiểm tra API hoạt động
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "Product API is working", timestamp = DateTime.Now });
        }

        // Test endpoint để kiểm tra database
        [HttpGet("test-db")]
        public async Task<IActionResult> TestDb()
        {
            try
            {
                var testResult = await _productService.TestDatabaseConnectionAsync();
                return Ok(testResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    message = "Database test failed", 
                    error = ex.Message,
                    stackTrace = ex.StackTrace 
                });
            }
        }

        // GET: api/Product/categories
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                Console.WriteLine("GetCategories called");
                var categories = await _productService.GetCategoriesAsync();
                Console.WriteLine($"GetCategories success, count: {categories}");
                return Ok(categories);
            }
            catch (Exception ex)
            {
                // Log full exception details
                var errorDetails = new
                {
                    message = "Lỗi khi lấy danh sách danh mục",
                    error = ex.Message,
                    stackTrace = ex.StackTrace,
                    innerException = ex.InnerException?.Message,
                    innerStackTrace = ex.InnerException?.StackTrace
                };
                Console.WriteLine($"ERROR GetCategories: {ex.GetType().Name} - {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner: {ex.InnerException.Message}");
                }
                return StatusCode(500, errorDetails);
            }
        }

        // GET: api/Product/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(long id)
        {
            try
            {
                var product = await _productService.GetProductAsync(id);
                if (product == null) return NotFound(new { message = "Không tìm thấy sản phẩm" });
                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thông tin sản phẩm", error = ex.Message });
            }
        }

        // POST: api/Product
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { message = "Dữ liệu không hợp lệ", errors });
            }

            try
            {
                var product = await _productService.CreateProductAsync(dto);
                var idProp = product.GetType().GetProperty("IdSanPham") ?? product.GetType().GetProperty("idSanPham");
                var id = idProp?.GetValue(product);
                return CreatedAtAction(nameof(GetProduct), new { id }, product);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/Product/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(long id, [FromBody] CreateProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { message = "Dữ liệu không hợp lệ", errors });
            }

            try
            {
                var product = await _productService.UpdateProductAsync(id, dto);
                if (product == null) return NotFound(new { message = "Không tìm thấy sản phẩm" });
                return Ok(product);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/Product/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(long id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);
                if (!result) return NotFound(new { message = "Không tìm thấy sản phẩm" });
                return Ok(new { message = "Xóa sản phẩm thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/Product/{id}/promotion
        [HttpPut("{id}/promotion")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetPromotion(long id, [FromBody] SetPromotionDto dto)
        {
            try
            {
                var result = await _productService.SetPromotionAsync(id, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}

