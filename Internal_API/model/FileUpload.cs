
using Internal_API.constants;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Internal_API.models
{
    [Table("FileUpload")]
    public class FileUpload
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(255)]
        public string filename { get; set; }

        [Required]
        public int year { get; set; }


        public DateTime uploaddate { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? category { get; set; }

        [MaxLength(50)]
        public string? filetype { get; set; }

        [Required]
        [MaxLength(500)]
        public string? s3_key { get; set; }

        [MaxLength(100)]
        public string? userinfo { get; set; }

        [MaxLength(50)]
        [Column(TypeName = "text")] // ✅ Ensure EF Core treats this as a TEXT field
        [JsonConverter(typeof(JsonStringEnumConverter))] // ✅ Ensures correct serialization
        public FileStatus? status { get; set; }

        [MaxLength(120)]
        public string? message { get; set; }
    }
}

