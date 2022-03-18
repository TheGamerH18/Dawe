using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dawe.Models
{
    public class Series
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Year { get; set; }
        public byte[] Thumbnail { get; set; }
        public SeriesTag Tag { get; set; } = new();
        [NotMapped]
        public List<Episode> Episodes { get; set; } = new();
    }

        public class SeriesTag
        {
            [Key]
            public int Id { get; set; }
            [Required]
            public string Name { get; set; }
        }
}
