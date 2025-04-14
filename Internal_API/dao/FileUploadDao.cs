using Internal_API.constants;
using Internal_API.models;
using Internal_API.Services;
using Microsoft.EntityFrameworkCore;
using System;

namespace Internal_API.data
{
    public class FileUploadDao : IFileUploadDao
    {
        private readonly AppDbContext dbContext;

        public FileUploadDao(AppDbContext context)
        {
            dbContext = context;
        }

        public IQueryable<FileUpload> getQuery()
        {
            var query = dbContext.FileUploads.AsQueryable();
            return query;
        }

        public IList<FileUpload> GetListUploadsByYear(int year)
        {
            
            // Return the list of records if they exist
            return dbContext.FileUploads.Where(a=>a.year == year).ToList();
        }

        public FileUpload? GetUploadById(Guid id)
        { 
            var result = dbContext.FileUploads.FirstOrDefault(a=>a.id == id);

            return result;
        }

        public IList<FileUpload> GetListUploadByUser(string user)
        {
            return dbContext.FileUploads.Where(a=>a.userinfo == user).ToList();
        }

        public IList<FileUpload> GetFileUploadsWithUserInfo()
        {
            
                var result = from f in dbContext.FileUploads
                             join u in dbContext.UserInfo
                             on f.userinfo equals u.username
                             select f;

                return result.ToList();
            
        }

        // In FileUploadDao implementation
        public void DeleteFileUpload(Guid fileId)
        {
            var file = dbContext.FileUploads.Find(fileId);
            if (file != null)
            {
                dbContext.FileUploads.Remove(file);
                dbContext.SaveChanges();
            }
        }


        public Guid SaveFileUpload(S3FileInfo fileInfo)
        {
            FileUpload upload = new FileUpload();
            upload.filename = fileInfo.fileName;
            upload.category = fileInfo.category;
            upload.s3_key = fileInfo.S3Key;
            upload.status = FileStatus.uploaded;
            upload.userinfo = fileInfo.userInfo;
            upload.year = fileInfo.year;
            upload.filetype = fileInfo.filetype;
            upload.message = "uploading completed";
            dbContext.FileUploads.Add(upload);
            dbContext.SaveChanges();
            return upload.id;
        }

    }
}
