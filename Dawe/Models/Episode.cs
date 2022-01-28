using System.ComponentModel.DataAnnotations;

namespace Dawe.Models
{
    public class Episode
    {
        [Key]
        public int episodeId { get; set; }
        public int episodeNumber { get; set; }
        [Required]
        public Show show { get; set; }
        public string name { get; set; }
        public string EpisodePath { get; set; }
        public byte[] Cover { get; set; }
    }
}
