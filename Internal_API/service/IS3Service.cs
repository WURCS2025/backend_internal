using Internal_API.models;

namespace Internal_API.Services
{
    public interface IS3Service
    {
        Task<string> UploadFileAsync(S3FileInfo fileInfo);
    }
}

