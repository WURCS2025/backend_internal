namespace Internal_API.models
{
    public class S3FileInfo
    {
        public IFormFile file { get; set; }  // File to be uploaded
        public string fileName { get; set; } // Name of the file
        public string type { get; set; }     // File type (e.g., image/png)
        public string userInfo { get; set; } // User-related metadata
        public int year { get; set; }        // Associated year
    }

}
