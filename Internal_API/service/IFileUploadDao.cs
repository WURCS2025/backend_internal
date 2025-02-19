﻿using Internal_API.models;

namespace Internal_API.Services
{
    public interface IFileUploadDao
    {
        Guid SaveFileUpload(S3FileInfo fileInfo);

        FileUpload? GetUploadById(Guid id);

        IList<FileUpload> GetListUploadsByYear(int year);
    }
}
