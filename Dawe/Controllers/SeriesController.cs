using Dawe.Data;
using Dawe.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Dawe.Controllers
{
    public class SeriesController : Controller
    {
        private readonly Data.DataContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<SeriesController> _logger;

        public SeriesController(DataContext context, IWebHostEnvironment environment, ILogger<SeriesController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }
 
        public async Task<IActionResult> Index()
        {
            var shows = await _context.Series.ToListAsync();
            _ = await _context.SeriesTag.ToListAsync();
            List<Series> result = new();
            result.AddRange(shows);
            return View(result);
        }

        // GET: /Series/Create
        public IActionResult Create()
        {
            UploadModel model = new();
            var tags = _context.SeriesTag.ToList();
            tags.ForEach(x => model.Categorys.Add(new SelectListItem(x.Name, x.Id.ToString())));

            return View(model);
        }

        // POST: /Series/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UploadModel upload)
        {
            if (!ModelState.IsValid) return BadRequest();
            var Tag = await _context.SeriesTag.FindAsync(int.Parse(upload.SelectedCategory));
            if (Tag == null) return BadRequest("Invalid Category");

            Series series = new()
            {
                Name = upload.Name,
                Tag = Tag,
                Description = upload.Description,
                Year = upload.Year,
                Thumbnail = ConvertCover(upload.CoverFile)
            };
            await _context.Series.AddAsync(series);
            _ = _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Series/CreateTag
        public IActionResult CreateTag()
        {
            return View();
        }

        // POST: /Series/CreateTag
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTag(CreateTagModel model)
        {
            if (!ModelState.IsValid) return BadRequest();
            SeriesTag tag = new()
            {
                Name = model.Tag
            };

            await _context.AddAsync(tag);
            _ = _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Series/Edit/
        public async Task<IActionResult> Edit(int? id)
        {
            _context.ChangeTracker.AutoDetectChangesEnabled = true;

            if (id == null) return BadRequest();
            var show = await GetShow((int)id);
            if (show == null) return BadRequest();
            var tags = await _context.SeriesTag.ToListAsync();
            
            var model = new EditModel()
            {
                Coverbyte = show.Thumbnail,
                Name = show.Name,
                Description = show.Description,
                Id = show.Id,
                Year = show.Year,
                SelectedCategory = show.Tag.Id.ToString(),
                Seasons = show.Seasons
            };

            tags.ForEach(tags => model.Categorys.Add(new SelectListItem(tags.Name, tags.Id.ToString())));

            return View(model);
        }

        // POST: /Series/Delete/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int? id)
        {
            if(id is null) return BadRequest();
            var series = await GetShow((int)id);
            if (series is null) return NotFound();

            series.Seasons.ForEach(x =>
            {
                _context.RemoveRange(x.Episodes);
                _context.Remove(x);
            });
            _context.Remove(series);

            return Ok();
        }

        // POST: /Series/AddSeason
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSeason(int? id)
        {
            if(id is null) return BadRequest();
            var series = await GetShow((int)id);
            if (series == null) return BadRequest();

            Season season = new()
            {
                Series = series,
                Seasonnumber = series.Seasons.Count() + 1,
            };

            await _context.Seasons.AddAsync(season);
            _ = _context.SaveChangesAsync();

            return Ok();
        }

        // POST: /Series/RemoveSeason
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSeason(int? id)
        {
            if(id is null) return BadRequest();
            var season = await _context.Seasons.FindAsync(id);
            if(season is null) return BadRequest();

            _context.Episodes.Where(x => x.season == season).ToListAsync().Result.ForEach(x =>
            {
                IFileHelper.DeleteFile(x.EpisodePath, _environment.WebRootPath);
                _context.Remove(x);
            });
            _context.Remove(season);
            _ = _context.SaveChangesAsync();

            return Ok();
        }

        // GET: /Series/AddEpisode/id
        public IActionResult AddEpisode(int? id)
        {
            if(id == null) return BadRequest();
            var episodesofshow = _context.Episodes.Where(x => x.season.Id == (int)id).ToList();
            int cnumber = episodesofshow.Count() + 1;
            EpisodeCreateModel model = new()
            {
                seasonid = (int)id,
                EpisodeNumber = cnumber
            };
            return View(model);
        }

        // POST: /Series/AddEpisode
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEpisode(EpisodeCreateModel model)
        {
            if(!ModelState.IsValid) return BadRequest();
            var season = await _context.Seasons.FindAsync(model.seasonid);
            if(season is null) return BadRequest();

            Episode episode = new()
            {
                name = model.name,
                EpisodePath = model.Path,
                episodeNumber = model.EpisodeNumber,
                season = season
            };
            await _context.Episodes.AddAsync(episode);
            _ = _context.SaveChangesAsync();
            _ = await _context.Series.ToListAsync();
            return RedirectToAction(nameof(Edit), new { id = season.Series.Id });
        }

        // GET: /Series/EditEpisode
        public IActionResult EditEpisode(int? id)
        {
            if(id == null) return BadRequest();
            var episode = _context.Episodes.Find((int)id);
            if(episode is null) return BadRequest();
            var model = new EpisodeEditModel()
            {
                id = (int)id,
                Name = episode.name,
                EpisodeNumber = episode.episodeNumber
            };
            return View(model);
        }

        // POST: /Series/EditEpisode
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditEpisode(EpisodeEditModel model)
        {
            var episode = _context.Episodes.Find(model.id);
            if (episode == null) return BadRequest();

            episode.name = model.Name;
            episode.episodeNumber = model.EpisodeNumber;

            _context.Episodes.Update(episode);
            _ = _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: /Series/DeleteEpisode
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEpisode(int? id)
        {
            if(id is null) return BadRequest();
            var episode = await _context.Episodes.FindAsync(id);
            if(episode is null) return NotFound();

            IFileHelper.DeleteFile(episode.EpisodePath, _environment.WebRootPath);
            _context.Remove(episode);
            _ = _context.SaveChangesAsync();
            return Ok();
        }

        public IActionResult WatchEpisode(int? id)
        {
            if (id is null) return BadRequest();
            var episode = _context.Episodes.Find(id);
            if (episode is null) return NotFound();
            EpisodeWatchModel model = new()
            {
                Name = episode.name,
                Id = episode.episodeId,
                EpisodeNumber = episode.episodeNumber,
            };
            return View(model);
        }

        public async Task<IActionResult> Episode(int? id)
        {
            if (id is null) return BadRequest();
            var episode = await _context.Episodes.FindAsync(id);
            if (episode is null) return BadRequest();
            var path = IFileHelper.GetPathAndFilename(episode.EpisodePath, _environment.WebRootPath);
            var content = System.IO.File.OpenRead(path);

            return File(content, DataValidation.GetEnumDescription(FileType.MP4), enableRangeProcessing: true);
        }

        // Series/Upload
        // Disabled Size Limit
        [HttpPost]
        [DisableRequestSizeLimit,
        RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue,
        ValueLengthLimit = int.MaxValue)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(IFormFile files)
        {
            if(files is null) return BadRequest();
            // Validate Extension
            if (!Data.DataValidation.Checkextension(Path.GetExtension(files.FileName)))
            {
                _logger.LogWarning("Invalid extension");
                return BadRequest();
            }
            // Create file name
            string filename = IFileHelper.CreateFilename(Path.GetExtension(files.FileName));

            // Create Path
            string path = IFileHelper.GetPathAndFilename(filename, _environment.WebRootPath);
            _logger.LogInformation(path);
            // Save File
            using (FileStream output = System.IO.File.Create(path))
                await files.CopyToAsync(output);
            return Ok(filename);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if(id != null)
            {
                var show = await GetShow((int)id);
                if(show != null)
                {
                    return View(show);
                } 
                else
                {
                    return NotFound();
                }
            }
            return BadRequest();
        }

        public async Task<Series?> GetShow(int id)
        {
            if(_context.Series.Any(m => m.Id == id))
            {
                // Get basic series
                var show = await _context.Series.FindAsync(id);
                if (show is null) return null;
                // Add category to series
                var categories = await _context.SeriesTag.ToListAsync();

                categories.ForEach(x =>
                {
                    if (x.Id == show.Id) show.Tag = x;
                });
                // Add Season to show
                _ = _context.Seasons.ToListAsync();
                var seasons = await _context.Seasons.Where(x => x.Series.Id == show.Id).ToListAsync();
                seasons.ForEach(async x =>
                {
                    // Add Episodes to Season
                    var episodes = await _context.Episodes.OrderBy(x => x.episodeNumber).ToListAsync();
                    episodes.ForEach(y =>
                    {
                        if(y.season.Id == x.Id)
                        {
                            x.Episodes.Add(y);
                        }
                    });
                    show.Seasons.Add(x);
                });

                return show;
            }
            return null;
        }

        private byte[] ConvertCover(IFormFile Cover)
        {
            Image img;
            if (Cover == null)
            {
                // Load placeholder image
                img = Image.Load(Path.Combine(Environment.CurrentDirectory, @"ressources\movieplaceholder.png"));
            }
            else
            {
                // Load Specified Image and resize
                img = Image.Load(Cover.OpenReadStream());
                img.Mutate(x => x.Resize(new ResizeOptions()
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(300, 450)
                }));
            }
            // Copy Image to array
            using MemoryStream memoryStream = new MemoryStream();
                img.SaveAsPng(memoryStream);
            return memoryStream.ToArray();
        }

        public class UploadModel
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public IFormFile CoverFile { get; set; }
            public string Year { get; set; }

            public string SelectedCategory { get; set; } = "1";
            public List<SelectListItem> Categorys { get; } = new List<SelectListItem>();
        }

        public class EditModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public byte[] Coverbyte { get; set; }
            public IFormFile Coverfile { get; set; }
            public string Year { get; set; }
            public List<Season> Seasons { get; set; }

            public string SelectedCategory { get; set; } = "1";
            public List<SelectListItem> Categorys { get; } = new List<SelectListItem>();
        }

        public class EpisodeCreateModel
        {
            public int seasonid { get; set; }
            public string Path { get; set; }
            public string name { get; set; }
            public int EpisodeNumber { get; set; }
        }

        public class EpisodeEditModel
        {
            public int id { get; set; }
            public string Name { get; set; }
            public int EpisodeNumber { get; set; }
        }

        public class EpisodeWatchModel
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public int EpisodeNumber { get; set; }
        }

        public class CreateTagModel
        {
            public string Tag { get; set; }
        }
    }
}
