

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Internal_API.models
{


    [Table("FileUpload", Schema = "public")]
    public class FileUpload
    {
        [Key]
        [MaxLength(36)]
        public string id { get; set; }

        [Required]
        public string filename { get; set; }

        public int year { get; set; }

        public DateTime? uploaddate { get; set; }

        public string category { get; set; }

        public string filetype { get; set; }

        [Required]
        public string s3_key { get; set; }

        public string userinfo { get; set; }

        public string status { get; set; }

        public string message { get; set; }
    }

    
   

}

