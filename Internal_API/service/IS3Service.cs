using Amazon.S3.Model;
using Internal_API.models;
using Microsoft.AspNetCore.Mvc;

namespace Internal_API.Services
{
    public interface IS3Service
    {
        Task<string> UploadFileAsync(S3FileInfo fileInfo);

        Task<GetObjectResponse> GetFileAsync(string s3key);
    }
}

