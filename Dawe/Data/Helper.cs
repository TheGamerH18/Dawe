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
        public static string GetPathAndFilename(string filename, string WebRootPath)
        {
            string subfolder = "uploads/";
            // return _hostingEnvironment.WebRootPath + "\\uploads\\" + filename;
            string path = Path.Combine(WebRootPath, subfolder);
            return Path.Combine(path, filename);
        }

        /// <summary>
        /// Checks if a uploaded file exists
        /// </summary>
        /// <param name="filename">Name of file to be checked</param>
        /// <param name="WebRootPath">WebRootPath from a IWebHostEnvironment</param>
        /// <returns>True, if the file exists</returns>
        public static bool FileExists(string filename, string WebRootPath)
        {
            string path = GetPathAndFilename(filename, WebRootPath);
            return File.Exists(path);
        }

        /// <summary>
        /// Deletes the specified uploaded File
        /// </summary>
        /// <param name="filename">Name of File to be deleted</param>
        /// <param name="WebRootPath">WebRootPath from a IWebHostEnvironment</param>
        /// <returns>True, the file is deleted successfully</returns>
        public static bool DeleteFile(string filename, string WebRootPath)
        {
            if(FileExists(filename, WebRootPath))
            {
                var path = GetPathAndFilename(filename, WebRootPath);
                File.Delete(path);
                return true;
            }
            return false;
        }
    }

    public class IImageHelper : Helper
    {

    }
}
