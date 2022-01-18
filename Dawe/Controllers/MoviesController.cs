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
            var movies = await _context.Movies.ToListAsync();
            foreach (var movie in movies)
            {
                var tags = await _context.Tags.Where(p => p.Movie == movie).Select(p => p.Tag).ToListAsync();
                movie.Tags.AddRange(tags);
            }
            movies.ForEach(mov => mov.Tags.ForEach(tags => _logger.LogInformation("Add Tag" + tags)));
            return View(movies);
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
                Image img;
                if(upload.CoverFile == null)
                {
                    // Load placeholder Image as Cover
                    _logger.LogWarning("No Cover Uploaded");
                    img = Image.Load(Path.Combine(Environment.CurrentDirectory, @"ressources\movieplaceholder.png"));
                } 
                else
                {
                    // Resize Uploaded Cover
                    img = Image.Load(upload.CoverFile.OpenReadStream());
                    img.Mutate(x => x.Resize(new ResizeOptions()
                    {
                        Mode = ResizeMode.BoxPad,
                        Size = new Size(300, 450)
                    }));
                }
                // Copy Cover to Stream
                using MemoryStream memoryStream = new MemoryStream();
                    img.SaveAsPng(memoryStream);

                // Save to model
                var movie = new Movies()
                {
                    Name = upload.Name,
                    MoviePath = upload.MoviePath,
                    ReleaseDate = upload.Date,
                    Cover = memoryStream.ToArray(),
                };

                // Save Tags
                var list = upload.Tags.Split(',').ToList();
                foreach (var stringtag in list)
                {
                    var tag = new Tags()
                    {
                        Movie = movie,
                        Tag = stringtag
                    };
                    _context.Add(tag);
                }

                _context.Add(movie);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Uploaded Movie " + movie.Name);
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

            var movies = await GetMovie((int)id);
            if(movies == null) return NoContent();
            var editmodel = new EditModel()
            {
                id = movies.Id,
                Name = movies.Name,
                Coverbyte = movies.Cover,
                Date = movies.ReleaseDate,
                Tags = ListtoString(movies.Tags)
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
                        // Delete Tags and Create Tags
                        DeleteTags(movie);
                        SaveTags(model.Tags, movie);
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

            var movies = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movies == null)
            {
                return NotFound();
            }

            var tags = await _context.Tags.Where(p => p.Movie == movies).Select(p => p.Tag).ToListAsync();
            movies.Tags.AddRange(tags);

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
            System.IO.File.Delete(GetPathAndFilename(movies.MoviePath));
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
            if (_context.Movies.Any(m => m.Id == id))
            {
                var movie = await _context.Movies.FindAsync(id);
                var tags = await _context.Tags.Where(tag => tag.Movie == movie).Select(tag => tag.Tag).ToListAsync();
                movie.Tags.AddRange(tags);
                return movie;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Reformates a List to a single string. Every Value is seperated with a ','
        /// </summary>
        /// <param name="list">List, that needs to be converted</param>
        /// <returns>Formated string</returns>
        private string ListtoString(List<string> list)
        {
            char[] en;
            var restring = "";
            for(int i = 0; i < list.Count - 1; i++)
            {
                en = list[i].ToCharArray();
                foreach(char c in en)
                {
                    restring.Append(c);
                }
                restring.Append(',');
            }
            en = list[list.Count - 1].ToCharArray();
            foreach (char c in en)
            {
                restring.Append(c);
            }
            return restring;
        }

        private async void SaveTags(string tags, Movies movie)
        {
            var list = CreateTags(tags, movie);
            await _context.AddRangeAsync(list);
            await _context.SaveChangesAsync();
        }

        private List<Tags> CreateTags(string tags, Movies movie)
        {
            var tagsList = new List<Tags>();
            var list = tags.Split(',').ToList();
            foreach (var stringtag in list)
            {
                var tag = new Tags()
                {
                    Movie = movie,
                    Tag = stringtag
                };
                tagsList.Add(tag);
            }
            return tagsList;
        }

        private async void DeleteTags(Movies movie)
        {
            var tags = _context.Tags.Where(tags => tags.Movie == movie).ToListAsync();
            _context.RemoveRange(tags);
            await _context.SaveChangesAsync();
        }

        private byte[] ConvertCover (IFormFile Cover)
        {
            var img = Image.Load(Path.Combine(Environment.CurrentDirectory, @"ressources\movieplaceholder.png"));
            using MemoryStream memoryStream = new MemoryStream();
                img.SaveAsPng(memoryStream);
            return memoryStream.ToArray();
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

    public class EditModel
    {
        public int id { get; set; }
        public byte[] Coverbyte { get; set; }
        [BindProperty]
        public IFormFile CoverFile { get; set; }
        public string Name { get; set; }
        public string Tags { get; set; }
        public string Date { get; set; }
    }
}
