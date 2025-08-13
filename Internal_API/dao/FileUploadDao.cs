using Internal_API.constants;
using Internal_API.models;
using Internal_API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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

        public async Task saveChanges()
        {
            await dbContext.SaveChangesAsync();
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

        public FileUpload? GetUploadById(string id)
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
            var file = dbContext.FileUploads.Find(fileId.ToString());
            if (file != null)
            {
                dbContext.FileUploads.Remove(file);
                dbContext.SaveChanges();
            }
        }


        public string SaveFileUpload(S3FileInfo fileInfo)
        {
            FileUpload upload = new FileUpload();
            upload.filename = fileInfo.fileName;
            upload.category = fileInfo.category;
            upload.s3_key = fileInfo.s3key;
            upload.status = FileStatus.uploaded.ToString();
            upload.userinfo = fileInfo.userInfo;
            upload.year = fileInfo.year ?? throw new ArgumentNullException(nameof(fileInfo.year));
            upload.filetype = fileInfo.filetype;
            upload.message = "uploading completed";
            upload.id = Guid.NewGuid().ToString();;
            dbContext.FileUploads.Add(upload);
            dbContext.SaveChanges();
            return upload.id;
        }

    }
}
