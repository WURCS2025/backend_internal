namespace Internal_API.model
{
    public class FileFilterRequest
    {
        public string? userid { get; set; }
        public string? year { get; set; }
        public string? filetype { get; set; }
        public string? category { get; set; }
        public string? status { get; set; }
        public string? sortfield { get; set; }
        public string? sortorder { get; set; }
        public bool? haserror { get; set; }
    }
}
