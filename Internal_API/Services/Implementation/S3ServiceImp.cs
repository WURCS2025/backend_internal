using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using Internal_API.models;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Internal_API.Services.Implementation
{
    class S3ServiceImp : IS3Service
    {
        private IAmazonS3 _s3Client;
        private IConfiguration configuration;

        async Task<string> IS3Service.UploadFileAsync(S3FileInfo fileInfo)
        {
            string? _bucketName = configuration["AWS:BucketName"];
            if (string.IsNullOrEmpty(_bucketName))
            {
                return "empty bucket";
            }
            var region = Amazon.RegionEndpoint.GetBySystemName(configuration["AWS:Region"]);
            _s3Client = new AmazonS3Client(region); // Uses default AWS credentials

            try
            {
                if (fileInfo.File == null || fileInfo.File.Length == 0)
                    throw new ArgumentException("Invalid file");

                using var memoryStream = new MemoryStream();
                await fileInfo.File.CopyToAsync(memoryStream);

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = memoryStream,
                    Key = $"{fileInfo.Year}/{fileInfo.UserInfo}/{fileInfo.FileName}",
                    BucketName = _bucketName,
                    ContentType = fileInfo.Type
                };

                var transferUtility = new TransferUtility(_s3Client);
                await transferUtility.UploadAsync(uploadRequest);

                return $"File uploaded successfully: {uploadRequest.Key}";
            }
            catch (Exception ex)
            {
                return "file failed";
                throw new Exception("Error uploading file to S3", ex);
                
            }
        }
    }
}
