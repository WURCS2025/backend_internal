using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Internal_API.model
{
    [Table("UserInfo")]
    public class UserInfo
    {

        public UserInfo(UserView userView) { 
        
           this.username = userView.username;
           this.lastname = userView.lastname;
           this.firstname = userView.firstname;
           this.email = userView.email;
            this.passwordhash = userView.password;
            this.userrole = userView.userrole;
        }

        public UserInfo()
        {

        }

        [Key]
        public Guid id { get; set; } = Guid.NewGuid();

        [Required]
        public string firstname { get; set; }

        [Required]
        public string lastname { get; set; }

        [Required]
        [EmailAddress]
        public string email { get; set; }

        [Required]
        public string username { get; set; }

        [Required]
        public string userrole { get; set; }

        [Required]
        public string passwordhash { get; set; }

        public DateTime createdate { get; set; } = DateTime.UtcNow;


    }

}
