#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Dawe.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Dawe.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            _context.ChangeTracker.AutoDetectChangesEnabled = true;
            var movies = await _context.Movies.ToListAsync();
            var tags = await _context.MovieTag.ToListAsync();

            return View(movies);
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movies = await GetMovie((int)id);
            if (movies == null)
            {
                return NotFound();
            }

            return View(movies);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            var tags = _context.MovieTag.ToList();
            UploadModel model = new();
            tags.ForEach(t => model.Categories.Add(new SelectListItem(t.Tag, t.Id.ToString())));
            return View(model);
        }
        
        // GET: /Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UploadModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            var tag = await _context.MovieTag.FindAsync(int.Parse(model.SelectedCategory));

            Movies movie = new()
            {
                Name = model.Name,
                MoviePath = model.MoviePath,
                Cover = ConvertCover(model.CoverFile),
                Tag = tag,
                ReleaseDate = model.Date
            };

            await _context.Movies.AddAsync(movie);
            _ = _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        public IActionResult CreateTag()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTag(CreateTagModel model)
        {
            if(!ModelState.IsValid) return BadRequest();
            MovieTag tag = new()
            {
                Tag = model.Tag,
            };

            await _context.AddAsync(tag);
            _ = _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Movies/Movie/Id
        public async Task<IActionResult> Movie(int? id)
        {
            if(id == null) return BadRequest();
            var movie = await GetMovie((int)id);
            if(movie == null) return NotFound();
            var path = IFileHelper.GetPathAndFilename(movie.MoviePath, _hostingEnvironment.WebRootPath);
            var content = System.IO.File.OpenRead(path);

            return File(content, "video/mp4", enableRangeProcessing: true);
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
            if (!Data.DataValidation.Checkextension(Path.GetExtension(files.FileName)))
            {
                return BadRequest();
            }
            // Create file name
            string filename = IFileHelper.CreateFilename(Path.GetExtension(files.FileName));

            // Create Path
            string path = IFileHelper.GetPathAndFilename(filename, _hostingEnvironment.WebRootPath);
            _logger.LogInformation(path);
            // Save File
            using (FileStream output = System.IO.File.Create(path))
                await files.CopyToAsync(output);

            return Ok(filename);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movies = await GetMovie((int)id);
            if(movies == null) return NoContent();
            var editmodel = new EditModel()
            {
                id = movies.Id,
                Name = movies.Name,
                Coverbyte = movies.Cover,
                Date = movies.ReleaseDate,
            };
            if (movies == null)
            {
                return NotFound();
            }
            return View(editmodel);
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditModel model)
        {
            if (ModelState.IsValid)
            {
                if(id == model.id)
                {
                    var movie = await GetMovie(id);
                    if (movie != null)
                    {
                        // Modify Movie
                        movie.Name = model.Name;
                        movie.ReleaseDate = model.Date;
                        if(model.CoverFile != null) { movie.Cover = ConvertCover(model.CoverFile); }
                        // Save to DB
                        _context.Update(movie);
                        await _context.SaveChangesAsync();
                        return View(model);
                    }
                    else
                    {
                        return BadRequest();
                    }
                }
                else
                {
                    return BadRequest();
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movies = await GetMovie((int)id);
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
            if (movies is null) return BadRequest();
            _context.Movies.Remove(movies);
            _ = _context.SaveChangesAsync();

            IFileHelper.DeleteFile(movies.MoviePath, _hostingEnvironment.WebRootPath);
            return RedirectToAction(nameof(Index));
        }

        private bool MoviesExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }

        /// <summary>
        /// Search is in the database for a movie with the specified id
        /// </summary>
        /// <param name="id">id of the movie</param>
        /// <returns>The movie or null if no movie was found</returns>
        private async Task<Movies> GetMovie(int id)
        {
            _context.ChangeTracker.AutoDetectChangesEnabled = true;
            if (_context.Movies.Any(m => m.Id == id))
            {
                var movie = await _context.Movies.FindAsync(id);
                var tags = await _context.MovieTag.FindAsync(movie.Tag);
                return movie;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Converts an IFormFile to Image, resizes it, and copies it to an byte array
        /// </summary>
        /// <param name="Cover">IFormFile, that is supossed to be converted</param>
        /// <returns>Converted byte array</returns>
        private byte[] ConvertCover (IFormFile Cover)
        {
            Image img;
            if(Cover == null)
            {
                // Load placeholder image
                img = Image.Load(Path.Combine(Environment.CurrentDirectory, @"ressources\movieplaceholder.png"));
            } else
            {
                // Load Specified Image and resize
                img = Image.Load(Cover.OpenReadStream());
                img.Mutate(x => x.Resize(new ResizeOptions()
                {
                    Mode = ResizeMode.BoxPad,
                    Size = new Size(300, 450)
                }));
            }
            // Copy Image to array
            using MemoryStream memoryStream = new MemoryStream();
                img.SaveAsPng(memoryStream);
            return memoryStream.ToArray();
        }

        public class CreateTagModel
        {
            public string Tag { get; set; }
        }
    }

    public class UploadModel
    {
        [BindProperty]
        public IFormFile CoverFile { get; set; }
        public string MoviePath { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }

        public string SelectedCategory { get; set; } = "1";
        public List<SelectListItem> Categories { get; } = new();
    }

    public class EditModel
    {
        public int id { get; set; }
        public byte[] Coverbyte { get; set; }
        [BindProperty]
        public IFormFile CoverFile { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }

        public string SelectedCategory { get; set; } = "1";
        public List<SelectListItem> Categories { get; } = new();
    }
}
