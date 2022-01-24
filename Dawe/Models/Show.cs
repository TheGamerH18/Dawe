using System.ComponentModel.DataAnnotations.Schema;

namespace Dawe.Models
{
    public class Show
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Year { get; set; }
        public byte[] Thumbnail { get; set; }
        [NotMapped]
        public List<string> Tags { get; set; }
    }
}
