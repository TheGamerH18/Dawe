using System.ComponentModel.DataAnnotations;

namespace Dawe.Models
{
    public class Movies
    {
        [Key]
        public int Id { get; set; }
        public string MoviePath { get; set; } = string.Empty;

        public byte[] Cover { get; set; }
        public string Name { get; set; } = string.Empty;

        public MovieTag Tag { get; set; }

        public string ReleaseDate { get; set; }
    }

    public class MovieTag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Tag { get; set; }
    }
}
