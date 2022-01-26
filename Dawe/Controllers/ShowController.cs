using Dawe.Data;
using Dawe.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var shows = await _context.Show.ToListAsync();
            
            return View(shows);
        }

        public async Task<Show?> GetShow(int id)
        {
            if(_context.Show.Any(m => m.Id == id))
            {
                var show = await _context.Show.FindAsync(id);
                var tags = await _context.ShowTags.Where(tag => tag.Show == show).Select(tag => tag.Tag).ToListAsync();
                show.Tags.AddRange(tags);
                return show;
            }
            else
            {
                return null;
            }
        }

        public class UploadModel
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public IFormFile CoverFile { get; set; }
            public string Tags { get; set; }
            public int EpisodeCount { get; set; }
            public string Year { get; set; }
        }
    }
}
