using Microsoft.AspNetCore.Mvc;

namespace Internal_API.models
{
    public class S3FileInfo
    {
        [FromForm(Name = "file")]
        public IFormFile file { get; set; }

        [FromForm(Name = "fileName")]
        public string fileName { get; set; }

        [FromForm(Name = "filetype")]
        public string filetype { get; set; }

        // If optional, keep it nullable to avoid auto-400; or keep int and ensure client always sends a number
        [FromForm(Name = "year")]
        public int? year { get; set; }

        [FromForm(Name = "category")]
        public string category { get; set; }

        // If you only need a string, keep it string. If it's complex, don't bind it here—see note below.
        [FromForm(Name = "userInfo")]
        public string userInfo { get; set; }

        [FromForm(Name = "s3key")]
        public string s3key { get; set; }
    }

}

