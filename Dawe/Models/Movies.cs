using SixLabors.ImageSharp;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Dawe.Models
{
    public class Movies
    {
        [Key]
        public int Id { get; set; }
        public string MoviePath { get; set; } = string.Empty;
        public byte[] Cover { get; set; } = null;
        public string Name { get; set; } = string.Empty;

        public List<string> Tags { get; } = new();

        public DateTime ReleaseDate { get; set; }
    }
}
