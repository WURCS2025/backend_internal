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

        public Guid SaveFileUpload(S3FileInfo fileInfo)
        {
            FileUpload upload = new FileUpload();
            upload.filename = fileInfo.fileName;
            upload.category = fileInfo.category;
            upload.s3_key = fileInfo.S3Key;
            upload.status = FileStatus.uploaded;
            upload.userinfo = fileInfo.userInfo;
            upload.year = fileInfo.year;
            upload.message = "uploading completed";
            dbContext.FileUploads.Add(upload);
            dbContext.SaveChanges();
            return upload.id;
        }

    }
}
