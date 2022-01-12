#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Dawe.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Dawe.Controllers
{
    public class MoviesController : Controller
    {
        private readonly Data.DataContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment; 
        private readonly ILogger<MoviesController> _logger;

        public MoviesController(Data.DataContext context, IWebHostEnvironment hostingEnvironment, ILogger<MoviesController> logger)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movies.ToListAsync());
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movies = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movies == null)
            {
                return NotFound();
            }

            return View(movies);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UploadModel upload)
        {
            if(ModelState.IsValid)
            {
                // Resize Cover
                Image img = Image.Load(upload.CoverFile.OpenReadStream());
                img.Mutate(x => x.Resize(new ResizeOptions()
                {
                    Mode = ResizeMode.BoxPad,
                    Size = new Size(300, 450)
                }));
                // Copy Cover to Stream
                using MemoryStream memoryStream = new MemoryStream();
                    img.SaveAsPng(memoryStream);
                _logger.LogInformation(upload.MoviePath);
                // Save to model
                var movie = new Movies()
                {
                    Name = upload.Name,
                    MoviePath = upload.MoviePath,
                    ReleaseDate = upload.Date,
                    Cover = memoryStream.ToArray(),
                };
                var split = upload.Tags.Split(',');
                var list = split.ToList();
                movie.Tags.AddRange(list);
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(upload);
        }

        // Post: Movies/Upload
        // Unlimited File Size
        [HttpPost]
        [DisableRequestSizeLimit,
        RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue,
        ValueLengthLimit = int.MaxValue)]
        public async Task<IActionResult> Upload(IFormFile files)
        {
            // Validate Extension
            if(!Data.DataValidation.Checkextension(Path.GetExtension(files.FileName)))
            {
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
            string path = Path.Combine(_hostingEnvironment.WebRootPath, subfolder);
            return Path.Combine(path, filename);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movies = await _context.Movies.FindAsync(id);
            if (movies == null)
            {
                return NotFound();
            }
            return View(movies);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MoviePath,Cover,Name,Tags,ReleaseDate")] Movies movies)
        {
            if (id != movies.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(movies);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MoviesExists(movies.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(movies);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movies = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movies == null)
            {
                return NotFound();
            }

            return View(movies);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movies = await _context.Movies.FindAsync(id);
            _context.Movies.Remove(movies);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MoviesExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }

    public class UploadModel
    {
        [BindProperty]
        public IFormFile CoverFile { get; set; }
        public string MoviePath { get; set; }
        public string Name { get; set; }
        public string Tags  { get; set; }
        public string Date { get; set; }
    }
}
