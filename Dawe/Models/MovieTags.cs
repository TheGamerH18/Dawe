using System.ComponentModel.DataAnnotations;

namespace Dawe.Models
{
    public class MovieTags
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Movies Movie { get; set; }

        [Required]
        public string Tag { get; set; }
    }
}
