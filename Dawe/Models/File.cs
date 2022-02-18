using Dawe.Data;
using System.ComponentModel.DataAnnotations;

namespace Dawe.Models
{
    public class File
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public FileType Type { get; set; } = FileType.NONE;
        public FileCategory? Category { get; set; }
    }

    public class FileCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
