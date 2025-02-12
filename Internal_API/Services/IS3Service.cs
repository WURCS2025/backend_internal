namespace Internal_API.Services
{
    public interface IS3Service
    {
        bool VerifyIdentity();

        Task<string> uploadfile(IFormFile file, string filename);
    }
}
