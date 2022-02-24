using Microsoft.AspNetCore.Mvc;
using Dawe.Models;
using Dawe.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            FileCreateModel model = new();
            List<FileCategory> categories = _context.FileCategories.ToList();
            categories.ForEach(x => model.Categorys.Add(new SelectListItem(x.Name, x.Id.ToString())));

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(FileCreateModel createModel)
        {
            _logger.LogInformation(createModel.selectedCategory);

            return RedirectToAction(nameof(Index));
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
            
            public string selectedCategory { get; set; }
            public List<SelectListItem> Categorys { get; set; } = new List<SelectListItem>();
        }

        public class CategoryCreateModel
        {
            public string Text { get; set; }
        }
    }
}
