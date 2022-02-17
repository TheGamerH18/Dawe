using Dawe.Data;
using Dawe.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Dawe.Controllers
{
    public class ShowController : Controller
    {
        private readonly Data.DataContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ShowController> _logger;

        public ShowController(DataContext context, IWebHostEnvironment environment, ILogger<ShowController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }



        public async Task<IActionResult> Index()
        {
            var shows = await _context.Shows.Select(s => s.Id).ToListAsync();
            List<Show> result = new();
            foreach(var id in shows)
            {
                result.Add(await GetShow(id));
            }
            return View(result);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UploadModel upload)
        {
            if(ModelState.IsValid)
            {
                var show = new Show()
                {
                    Name = upload.Name,
                    Description = upload.Description,
                    Year = upload.Year,
                    Thumbnail = ConvertCover(upload.CoverFile),
                };
                var tags = CreateTags(upload.Tags);
                SaveTags(tags, show);

                await _context.Shows.AddAsync(show);
                _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(upload);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return BadRequest();
            var show = await GetShow((int)id);

            var model = new EditModel()
            {
                Coverbyte = show.Thumbnail,
                Name = show.Name,
                Description = show.Description,
                Id = show.Id,
                Year = show.Year,
                Tags = ListtoString(show.Tags),
                Episodes = show.Episodes
            };
            return View(model);
        }

        // /Shows/AddEpisode
        public IActionResult AddEpisode(int? id)
        {
            if(id == null) return BadRequest();
            EpisodeCreateModel model = new()
            {
                showid = (int)id
            };
            return View(model);
        }

        // POST: /Shows/AddEpisode
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEpisode(EpisodeCreateModel model)
        {
            if(!ModelState.IsValid) return BadRequest();

            Episode episode = new()
            {
                name = model.name,
                EpisodePath = model.Path,
                episodeNumber = model.EpisodeNumber,
                show = await GetShow(model.showid)
            };
            await _context.Episodes.AddAsync(episode);
            _context.SaveChangesAsync();
            return await Edit(model.showid);
        }

        // Shows/Upload
        // Disabled Size Limit
        [HttpPost]
        [DisableRequestSizeLimit,
        RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue,
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

        public async Task<Show> GetShow(int id)
        {
            if(_context.Shows.Any(m => m.Id == id))
            {
                var show = await _context.Shows.FindAsync(id);
                var tags = await _context.ShowTags.Where(tag => tag.Show == show).Select(tag => tag.Tag).ToListAsync();
                if (tags.Any())
                {
                    show.Tags.AddRange(tags);
                }
                var episodes = await GetEpisodesAsync(show);
                if(episodes != null){
                    show.Episodes.AddRange(episodes);
                }
                return show;
            }
            return null;
        }

        public async Task<List<Episode>?> GetEpisodesAsync(Show show)
        {
            if(_context.Episodes.Any(m => m.show == show))
            {
                var episodes = _context.Episodes.Where(e => e.show == show).ToListAsync();
                return await episodes;
            }
            return null;
        }


        private List<string> CreateTags(string Tags)
        {
            var taglist = Tags.Split(',').ToList();
            taglist.ForEach(tag => tag.Trim());
            return taglist;
        }

        private async void SaveTags(List<string> tags, Show show)
        {
            var taglist = new List<ShowTags>();
            foreach (var tag in tags)
            {
                taglist.Add(new ShowTags()
                {
                    Tag = tag,
                    Show = show,
                });
            }
            await _context.ShowTags.AddRangeAsync(taglist);
            _context.SaveChangesAsync();
        }
        private string ListtoString(List<string> list)
        {
            if (list.Count == 0)
            {
                _logger.LogWarning("List Empty");
                return string.Empty;
            }
            var newstring = string.Join(", ", list);
            return newstring;
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
            public string Tags { get; set; }
            public string Year { get; set; }
        }

        public class EditModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public byte[] Coverbyte { get; set; }
            public IFormFile Coverfile { get; set; }
            public string Tags { get; set; }
            public string Year { get; set; }
            public List<Episode> Episodes { get; set; }

        }

        public class EpisodeCreateModel
        {
            public int showid { get; set; }
            public string Path { get; set; }
            public string name { get; set; }
            public int EpisodeNumber { get; set; }
        }
    }
}
