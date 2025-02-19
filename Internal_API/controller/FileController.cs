using Microsoft.AspNetCore.Mvc;
using Internal_API.Services;
using Internal_API.models;
using Internal_API.Services.Implementation;
using Microsoft.Extensions.Configuration;
using Amazon.S3;
using Internal_API.data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Internal_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class S3FileController : ControllerBase
    {
        private readonly IS3Service _s3FileService;
        private readonly IConfiguration _configuration;
        private readonly IFileUploadDao fileUploadDao;

        public S3FileController(IConfiguration configuration, IFileUploadDao fileUploadDao)
        {
            _configuration = configuration;
            var region = Amazon.RegionEndpoint.GetBySystemName(configuration["AWS:Region"]);
            var accessKey = configuration["AWS:AccessKey"];
            var secretKey = configuration["AWS:SecretKey"];
            IAmazonS3 _s3Client = new AmazonS3Client(accessKey, secretKey, region);
            _s3FileService = new S3ServiceImp(configuration, _s3Client);
            this.fileUploadDao = fileUploadDao;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] S3FileInfo fileInfo)
        {
            try
            {
                var temp = fileInfo.fileName;
                fileInfo.fileName = fileInfo.file.FileName;
                var result = await _s3FileService.UploadFileAsync(fileInfo);
                this.fileUploadDao.SaveFileUpload(fileInfo);
                return Ok(new { Message = result });
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

    }
}
