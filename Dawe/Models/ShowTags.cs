using System.ComponentModel.DataAnnotations;

namespace Dawe.Models
{
    public class ShowTags
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public Show Show { get; set; }
        [Required]
        public string Tag { get; set; }
    }
}
