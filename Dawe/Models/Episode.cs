using System.ComponentModel.DataAnnotations;

namespace Dawe.Models
{
    public class Episode
    {
        [Key]
        public int episodeId { get; set; }
        public int episodeNumber { get; set; }
        [Required]
        public Season season { get; set; }
        public string name { get; set; }
        public string EpisodePath { get; set; }
    }
}
