using System.ComponentModel.DataAnnotations;

namespace Internal_API.model
{
    public class UserView
    {
        public string id { get; set; }
        public string firstname { get; set; }        
        public string lastname { get; set; }
        public string email { get; set; }
        public string username { get; set; }
        public string userrole { get; set; }
        public string password { get; set; }

        // for json mapping
        public UserView()
        {

        }

        public UserView(UserInfo userInfo)
        {
            id = userInfo.id.ToString();
            firstname = userInfo.firstname;
            lastname = userInfo.lastname;
            email = userInfo.lastname;
            username = userInfo.username;
            userrole = userInfo.userrole;
        }
    }
}
