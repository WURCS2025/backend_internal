namespace Internal_API.model
{
    public class AwsOptions
    {
        public string AccessKeyId { get; set; }
        public string SecretAccessKey { get; set; }
        public string Region { get; set; }
        public string OutputDataSQSUrl { get; set; }

        public static AwsOptions readFromEnv()
        {
            AwsOptions awsOptions = new AwsOptions();
            awsOptions.AccessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            awsOptions.SecretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");
            awsOptions.Region = Environment.GetEnvironmentVariable("AWS_REGION");

            return awsOptions;
        }
    }

}
