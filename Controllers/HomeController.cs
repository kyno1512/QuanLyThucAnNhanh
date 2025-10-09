using Microsoft.AspNetCore.Mvc;

namespace QuanLyThucAnNhanh.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
