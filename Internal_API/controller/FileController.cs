﻿using Microsoft.AspNetCore.Mvc;
using Internal_API.Services;
using Internal_API.models;
using Internal_API.Services.Implementation;
using Microsoft.Extensions.Configuration;
using Amazon.S3;
using Internal_API.data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Collections;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Internal_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class S3FileController : ControllerBase
    {
        private readonly IS3Service s3FileService;
        private readonly IConfiguration configuration;
        private readonly IFileUploadDao fileUploadDao;

        public S3FileController(IConfiguration configuration, IFileUploadDao fileUploadDao)
        {
            this.configuration = configuration;
            var region = Amazon.RegionEndpoint.GetBySystemName(configuration["AWS:Region"]);
            var accessKey = configuration["AWS:AccessKey"];
            var secretKey = configuration["AWS:SecretKey"];
            IAmazonS3 s3Client = new AmazonS3Client(accessKey, secretKey, region);
            s3FileService = new S3ServiceImp(configuration, s3Client);
            this.fileUploadDao = fileUploadDao;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] S3FileInfo fileInfo)
        {
            try
            {
                var temp = fileInfo.fileName;
                fileInfo.fileName = fileInfo.file.FileName;
                var result = await s3FileService.UploadFileAsync(fileInfo);
                var id = this.fileUploadDao.SaveFileUpload(fileInfo);
                return Ok(new { Message = result  + "file id = " + id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("status")]
        public FileUpload? GetStatus(Guid id)
        {
            return fileUploadDao.GetUploadById(id);
            
        }

        [HttpGet("year")]
        public IList<FileUpload> GetFilesByYear(int year)
        {
            return fileUploadDao.GetListUploadsByYear(year);

        }

        [HttpGet("user")]
        public IList<FileUpload> GetFilesByUser(string userId)
        {
            IList<FileUpload> uploadList = fileUploadDao.GetListUploadByUser(userId);

            return uploadList;
        }

    }
}
