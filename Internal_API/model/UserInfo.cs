using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Internal_API.model
{
    [Table("UserInfo", Schema = "public")]
    public class UserInfo
    {
        [Key]
        [MaxLength(36)]
        public string id { get; set; }

        [Required]
        [MaxLength(50)]
        public string firstname { get; set; }

        [Required]
        [MaxLength(50)]
        public string lastname { get; set; }

        [Required]
        [MaxLength(100)]
        public string email { get; set; }

        [Required]
        [MaxLength(50)]
        public string username { get; set; }

        [Required]
        [MaxLength(256)]
        public string passwordhash { get; set; }

        public DateTime? createdate { get; set; }

        [MaxLength(50)]
        public string userrole { get; set; }

        public UserInfo(UserView userView)
        {

            this.username = userView.username;
            this.lastname = userView.lastname;
            this.firstname = userView.firstname;
            this.email = userView.email;
            this.passwordhash = userView.password;
            this.userrole = userView.userrole;
            this.id = Guid.NewGuid().ToString();;
        }

        public UserInfo()
        {

        }
     
    }

  

}
