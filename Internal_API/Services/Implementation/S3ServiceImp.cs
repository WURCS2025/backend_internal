using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
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

        async Task<string> IS3Service.uploadfile(IFormFile file, string filename)
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
                var fileTransferUtility = new TransferUtility(_s3Client);
                await fileTransferUtility.UploadAsync(file.OpenReadStream(), _bucketName, filename);

                return "file uploaded successfully";
            }
            catch (Exception ex)
            {
                throw new Exception("Error uploading file to S3", ex);
                return "file failed";
            }
        }

        bool IS3Service.VerifyIdentity()
        {
            return true;
        }
    }
}
