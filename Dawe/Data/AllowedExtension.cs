using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Dawe.Data
{
    public static class DataValidation
    {
        private static readonly string[] allowedextensions = new string[] { ".mp4" };

        public static bool Checkextension(string extension)
        {
            return allowedextensions.Contains(extension);
        }
    }
}
