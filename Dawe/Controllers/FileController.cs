using Microsoft.AspNetCore.Mvc;
using Dawe.Models;
using Dawe.Data;

namespace Dawe.Controllers
{
    public class FileController : Controller
    {

        private readonly DataContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ShowController> _logger;

        public FileController(DataContext context, IWebHostEnvironment environment, ILogger<ShowController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var files = _context.Files.ToList();
            return View(files);
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult CreateCategory()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(CategoryCreateModel createModel)
        {
            FileCategory category = new() { Name = createModel.Text };
            await _context.FileCategories.AddAsync(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public class FileCreateModel
        {
            public string Name { get; set; }
            public string Path { get; set; }
        }

        public class CategoryCreateModel
        {
            public string Text { get; set; }
        }
    }
}
