using SixLabors.ImageSharp;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Dawe.Models
{
    public class Movies
    {
        [Key]
        public int Id { get; set; }
        public string MoviePath { get; set; } = string.Empty;

        public byte[] Cover { get; set; }
        public string Name { get; set; } = string.Empty;

        [NotMapped]
        public List<string> Tags { get; } = new();

        public string ReleaseDate { get; set; }
    }
}
