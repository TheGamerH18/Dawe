using Microsoft.AspNetCore.Mvc;
using Dawe.Models;
using Dawe.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Dawe.Controllers
{
    public class FileController : Controller
    {

        private readonly DataContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ShowController> _logger;

        public async Task<IActionResult> Download(int? id)
        {
            if (id is null) return BadRequest();
            var file = await _context.Files.FindAsync(id);
            if(file is null) return NotFound();
            var path = IFileHelper.GetPathAndFilename(file.Path, _environment.WebRootPath);
            var content = await System.IO.File.ReadAllBytesAsync(path);

            
            return File(content, DataValidation.GetEnumDescription(file.Type));
        }

        public FileController(DataContext context, IWebHostEnvironment environment, ILogger<ShowController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var files = _context.Files.ToList();
            List<FileCategory> categories = _context.FileCategories.ToList();

            foreach (var file in files)
            {
                categories.ForEach(x =>
                {
                    if (x == file.Category) file.Category = x;
                });
            }

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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FileCreateModel createModel)
        {
            FileCategory? fileCategory = await _context.FileCategories.FindAsync(int.Parse(createModel.SelectedCategory));
            if (fileCategory is null) return BadRequest();

            if (!Enum.TryParse(Path.GetExtension(createModel.Path)[1..].ToUpper(), out FileType result)) return BadRequest();

            if (createModel.Name.Length == 0) return BadRequest();

            Models.File file = new()
            {
                Name = createModel.Name,
                Category = fileCategory,
                Path = createModel.Path,
                Type = result
            };

            await _context.Files.AddAsync(file);
            _ = _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if(id is null) return BadRequest();
            var file = await getFile((int)id);
            if(file is null) return NoContent();

            var dic = new Dictionary<string, string>();
            dic.Add("Id", file.Id.ToString());
            dic.Add("Name", file.Name);
            if(file.Category is null) dic.Add("Category", null);
            else dic.Add("Category", file.Category.Name);
            dic.Add("Type", file.Type.ToString());
            return Ok(dic);
        }

        public async Task<IActionResult> EditFile(int? id)
        {
            if(id is null) return BadRequest();
            var file = await getFile((int)id);
            if (file is null) return NoContent();

            FileEditModel editmodel = new()
            {
                Name = file.Name,
                Id = file.Id,
                SelectedCategory = file.Category.Name
            };

            List<FileCategory> categories = _context.FileCategories.ToList();
            categories.ForEach(x => editmodel.Categorys.Add(new SelectListItem(x.Name, x.Id.ToString())));

            return View(editmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFile(FileEditModel editModel)
        {
            if(!ModelState.IsValid) return BadRequest();
            var file = await getFile(editModel.Id);
            if (file is null) return BadRequest();

            FileCategory? fileCategory = await _context.FileCategories.FindAsync(int.Parse(editModel.SelectedCategory));
            if (fileCategory is null) return BadRequest();


            file.Name = editModel.Name;
            file.Category = fileCategory;

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFile(int? id)
        {
            if(id is null) return BadRequest();
            var file = await _context.Files.FindAsync(id);
            if (file is null) return NotFound();

            IFileHelper.DeleteFile(file.Path, _environment.WebRootPath);
            _context.Remove(file);
            _ = _context.SaveChangesAsync();

            return Ok();
        }

        // File/Upload
        // Disabled Size Limit
        [HttpPost]
        [DisableRequestSizeLimit,
        RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue,
        ValueLengthLimit = int.MaxValue)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile files)
        {
            // Validate Extension
            var extension = Path.GetExtension(files.FileName);
            if (extension is null) return BadRequest();
            if (!Enum.TryParse(extension[1..].ToUpper(), out FileType _))
            {
                _logger.LogWarning("invalid extension " + extension);
                return BadRequest();
            }
            // Create file name
            string filename = IFileHelper.CreateFilename(extension);

            // Create Path
            string path = IFileHelper.GetPathAndFilename(filename, _environment.WebRootPath);
            // Save File
            using (FileStream output = System.IO.File.Create(path))
                await files.CopyToAsync(output);
            return Ok(filename);
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

        private async Task<Models.File?> getFile(int fileid)
        {
            var file = await _context.Files.FindAsync(fileid);
            if (file is null) return null;
            List<FileCategory> categories = _context.FileCategories.ToList();

            categories.ForEach(x =>
            {
                if (x == file.Category) file.Category = x;
            });

            return file;
        }

        public class FileCreateModel
        {
            [Display(Name = "File Name")]
            public string Name { get; set; } = string.Empty;
            [Required]
            public string Path { get; set; } = string.Empty;

            public string SelectedCategory { get; set; } = "1";
            public List<SelectListItem> Categorys { get; set; } = new List<SelectListItem>();
        }

        public class FileEditModel
        {
            public int Id { get; set; }
            [Display(Name = "FileName")]
            public string Name { get; set; } = string.Empty;
            public string SelectedCategory { get; set; } = "1";
            public List<SelectListItem> Categorys { get; } = new List<SelectListItem>();
        }

        public class CategoryCreateModel
        {
            [Required]
            public string Text { get; set; } = string.Empty;
        }
    }
}
