namespace Internal_API.model
{
    public class SendMessage
    {
        string fileId {  get; set; }

        public string fileName { get; set; }

        public string s3_key { set; get; }

        public string category { set; get; }

        public string filetype { set; get; }

        public bool isUpdate { set; get; }

    }
}
