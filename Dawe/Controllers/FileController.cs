using Microsoft.AspNetCore.Mvc;

namespace Dawe.Controllers
{
    public class FileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
