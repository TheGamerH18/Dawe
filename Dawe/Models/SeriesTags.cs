using System.ComponentModel.DataAnnotations;

namespace Dawe.Models
{
    public class SeriesTags
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public Series Series { get; set; }
        [Required]
        public string Tag { get; set; }
    }
}
