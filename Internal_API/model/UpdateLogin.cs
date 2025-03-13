namespace Internal_API.model
{
    public class UpdateLogin : Login
    {
        public string newPwd {  get; set; }
        public string confirmPwd { get; set; }
    }
}
