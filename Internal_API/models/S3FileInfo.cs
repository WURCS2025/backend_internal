namespace Internal_API.models
{
    public class S3FileInfo
    {
        public IFormFile File { get; set; }  // File to be uploaded
        public string FileName { get; set; } // Name of the file
        public string Type { get; set; }     // File type (e.g., image/png)
        public string UserInfo { get; set; } // User-related metadata
        public int Year { get; set; }        // Associated year
    }

}
