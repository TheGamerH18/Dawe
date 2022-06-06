using System.ComponentModel;
using System.Reflection;

namespace Dawe.Data
{
    public static class DataValidation
    {
        private static readonly string[] allowedextensions = new string[] { ".mp4" };

        public static bool Checkextension(string extension)
        {
            return allowedextensions.Contains(extension);
        }

        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }
    }

    public enum FileType
    {
        NONE,

        [Description("video/mp4")]
        MP4,

        [Description("audio/mpeg")]
        MP3,

        [Description("audio/x-wav")]
        WAV,

        [Description("application/zip")]
        ZIP,

        [Description("application/pdf")]
        PDF,

        [Description("application/octet-stream")]
        ISO,

        [Description("application/x-rar-compressed")]
        RAR
    }
}
