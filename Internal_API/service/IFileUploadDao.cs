using Internal_API.models;

namespace Internal_API.Services
{
    public interface IFileUploadDao
    {
        Guid SaveFileUpload(S3FileInfo fileInfo);

        FileUpload? GetUploadById(Guid id);

        IList<FileUpload> GetListUploadsByYear(int year);

        IList<FileUpload> GetListUploadByUser(string user);

        IQueryable<FileUpload> getQuery();

        IList<FileUpload> GetFileUploadsWithUserInfo();

        Task saveChanges();

        void DeleteFileUpload(Guid fileId);


    }
}
