using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Internal_API.models;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;

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
                _s3Client = new AmazonS3Client(region); // Uses default AWS credentials
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

                //var uploadRequest = new TransferUtilityUploadRequest
                //{
                //    InputStream = memoryStream,
                //    Key = $"{fileInfo.year}/{fileInfo.type}/{fileInfo.fileName}/{timestring}",
                //    BucketName = _bucketName,
                //    ContentType = fileInfo.type
                //};

                //var transferUtility = new TransferUtility(_s3Client);
                //await transferUtility.UploadAsync(uploadRequest);

                //return $"File uploaded successfully: {uploadRequest.Key}";

                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = $"{fileInfo.year}/{fileInfo.type}/{fileInfo.fileName}",
                    InputStream = memoryStream,
                    ContentType = fileInfo.type
                };

                await _s3Client.PutObjectAsync(putRequest);
                return $"File uploaded successfully: {putRequest.Key}";
            }
            catch (Exception ex)
            {
                    
                throw new Exception("Error uploading file to S3", ex);

            }
        }

        
    }
}
