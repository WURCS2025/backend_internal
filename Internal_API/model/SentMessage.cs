using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Internal_API.model
{
    [Table("message", Schema = "public")]
    public class SentMessage
    {
        [Key]
        [MaxLength(36)]
        public string Id { get; set; }

        [Required]
        [MaxLength(36)]
        public string FileId { get; set; }

        [Required]
        public string MessageText { get; set; }

        [Required]
        public string Sender { get; set; }

        [Required]
        public string Receiver { get; set; }

        public DateTime? Date { get; set; }
    }
}
