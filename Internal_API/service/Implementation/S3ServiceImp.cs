
using Amazon.S3;
using Amazon.S3.Model;
using Internal_API.models;
using Internal_API.constants;

namespace Internal_API.Services.Implementation
{
    public class S3ServiceImp : IS3Service
    {
        private IAmazonS3? _s3Client;
        private string? _bucketName;
        private IConfiguration configuration;

        public S3ServiceImp(IConfiguration configuration, IAmazonS3 s3Client)
        {
            this.configuration = configuration;
            _bucketName = configuration["AWS:BucketName"];
            if (s3Client != null)
            {
                this._s3Client = s3Client;
            }
            else if (!string.IsNullOrEmpty(_bucketName))
            {
                var region = Amazon.RegionEndpoint.GetBySystemName(configuration["AWS:Region"]);
                var accessKey = configuration["AWS:AccessKey"];
                var secretKey = configuration["AWS:SecretKey"];
                _s3Client = new AmazonS3Client(accessKey, secretKey, region);
            }
            
        }

        async Task<string> IS3Service.UploadFileAsync(S3FileInfo fileInfo)
        {
                
            if (string.IsNullOrEmpty(_bucketName))
            {
                return "empty bucket";
            }            

            if (fileInfo.file == null || fileInfo.file.Length == 0)
                throw new ArgumentException("Invalid file");

            try
            {               

                using var memoryStream = new MemoryStream();
                await fileInfo.file.CopyToAsync(memoryStream);
                string timestring = DateTime.Now.ToShortTimeString();



                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = $"{fileInfo.year}/{fileInfo.category}/{timestring}-{fileInfo.fileName}",
                    InputStream = memoryStream,
                    ContentType = MimeTypes.Text,
                };

                await _s3Client.PutObjectAsync(putRequest);
                fileInfo.S3Key = putRequest.Key;
                return $"File uploaded successfully: {putRequest.Key}";
            }
            catch (Exception ex)
            {
                    
                throw new Exception("Error uploading file to S3", ex);

            }
        }

        
    }
}
