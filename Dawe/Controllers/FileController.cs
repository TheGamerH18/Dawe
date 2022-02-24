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

        // File/Upload
        // Disabled Size Limit
        [HttpPost]
        [DisableRequestSizeLimit,
        RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue,
        ValueLengthLimit = int.MaxValue)]
        public async Task<IActionResult> Upload(IFormFile files)
        {
            // Validate Extension
            if (!Data.DataValidation.Checkextension(Path.GetExtension(files.FileName)))
            {
                _logger.LogWarning("Invalid extension");
                return BadRequest();
            }
            // Create file name
            string filename = createFilename(Path.GetExtension(files.FileName));

            // Create Path
            string path = GetPathAndFilename(filename);
            _logger.LogInformation(path);
            // Save File
            using (FileStream output = System.IO.File.Create(path))
                await files.CopyToAsync(output);
            return Ok(filename);
        }

        // Create Unique File name and keeping the File extension
        private string createFilename(string fileextension)
        {
            return $@"{Guid.NewGuid()}{fileextension}"; ;
        }

        // Create Full Path to File
        private string GetPathAndFilename(string filename)
        {
            string subfolder = "uploads/";
            // return _hostingEnvironment.WebRootPath + "\\uploads\\" + filename;
            string path = Path.Combine(_environment.WebRootPath, subfolder);
            return Path.Combine(path, filename);
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
