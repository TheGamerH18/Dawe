﻿using Dawe.Data;
using Dawe.Models;
using Microsoft.AspNetCore.Mvc;
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
            var shows = await _context.Series.Select(s => s.Id).ToListAsync();
            List<Series> result = new();
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
                var show = new Series()
                {
                    Name = upload.Name,
                    Description = upload.Description,
                    Year = upload.Year,
                    Thumbnail = ConvertCover(upload.CoverFile),
                };
                var tags = CreateTags(upload.Tags);
                SaveTags(tags, show);

                await _context.Series.AddAsync(show);
                _ = _context.SaveChangesAsync();

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

        // /Series/AddEpisode
        public IActionResult AddEpisode(int? id)
        {
            if(id == null) return BadRequest();
            EpisodeCreateModel model = new()
            {
                showid = (int)id
            };
            return View(model);
        }

        // POST: /Series/AddEpisode
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEpisode(EpisodeCreateModel model)
        {
            if(!ModelState.IsValid) return BadRequest();
            var show = await GetShow(model.showid);
            if(show is null) return BadRequest();

            Episode episode = new()
            {
                name = model.name,
                EpisodePath = model.Path,
                episodeNumber = model.EpisodeNumber,
                show = show
            };
            await _context.Episodes.AddAsync(episode);
            _ = _context.SaveChangesAsync();
            return RedirectToAction(nameof(Edit), new { id = model.showid });
        }

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
            var content = await System.IO.File.ReadAllBytesAsync(path);

            return File(content, DataValidation.GetEnumDescription(Data.FileType.MP4));
        }

        // Series/Upload
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
                var show = await _context.Series.FindAsync(id);
                if (show is null) return null;
                var tags = await _context.SeriesTags.Where(tag => tag.Series == show).Select(tag => tag.Tag).ToListAsync();
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

        public async Task<List<Episode>?> GetEpisodesAsync(Series show)
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

        private async void SaveTags(List<string> tags, Series show)
        {
            var taglist = new List<SeriesTags>();
            foreach (var tag in tags)
            {
                taglist.Add(new SeriesTags()
                {
                    Tag = tag,
                    Series = show,
                });
            }
            await _context.SeriesTags.AddRangeAsync(taglist);
            _ = _context.SaveChangesAsync();
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
    }
}