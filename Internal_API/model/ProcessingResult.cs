namespace Internal_API.model
{
    public class ProcessingResult
    {
        public string id { get; set; }
        public string filename { get; set; }
        public string status { get; set; }
        public string process_result { get; set; } // Maps to "process_result"
        public string message { get; set; }
        public string error { get; set; }
    }

}
