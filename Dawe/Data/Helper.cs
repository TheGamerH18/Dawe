namespace Dawe.Data
{
    public interface Helper
    {
    }

    public class IFileHelper : Helper
    {
        // Create Unique File name and keeping the File extension
        public static string CreateFilename(string fileextension)
        {
            return $@"{Guid.NewGuid()}{fileextension}"; ;
        }

        // Create Full Path to File
        public  static string GetPathAndFilename(string filename, string WebRootPath)
        {
            string subfolder = "uploads/";
            // return _hostingEnvironment.WebRootPath + "\\uploads\\" + filename;
            string path = Path.Combine(WebRootPath, subfolder);
            return Path.Combine(path, filename);
        }
    }

    public class IImageHelper : Helper
    {

    }
}
