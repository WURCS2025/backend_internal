using Microsoft.AspNetCore.Mvc;
using Internal_API.Services;
using Internal_API.models;
using Internal_API.Services.Implementation;
using Amazon.S3;
using Microsoft.EntityFrameworkCore;
using Internal_API.model;
using Internal_API.constants;
using Internal_API.service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Internal_API.Controllers
{
    [IgnoreAntiforgeryToken]
    [Route("api/[controller]")]
    [ApiController]
    public class S3FileController : ControllerBase
    {
        private readonly IS3Service s3FileService;
        private readonly IConfiguration configuration;
        private readonly IFileUploadDao fileUploadDao;
        private readonly IUserInfoDao userInfoDao;

        public S3FileController(IConfiguration configuration, IFileUploadDao fileUploadDao, IUserInfoDao userInfoDao)
        {
            this.configuration = configuration;
            var region = Amazon.RegionEndpoint.GetBySystemName(configuration["AWS:Region"]);
            var accessKey = configuration["AWS:AccessKey"];
            var secretKey = configuration["AWS:SecretKey"];
            IAmazonS3 s3Client = new AmazonS3Client(accessKey, secretKey, region);
            s3FileService = new S3ServiceImp(configuration, s3Client);
            this.fileUploadDao = fileUploadDao;
            this.userInfoDao = userInfoDao;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] S3FileInfo fileInfo)
        {

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState); // <-- you’ll now see the exact binder errors

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
        public FileUpload? GetStatus(string id)
        {
            return fileUploadDao.GetUploadById(id);
            
        }

        [HttpGet("download")]
        public async Task<IActionResult> GetFileAsync(string fileId)
        {
            // Retrieve the S3 key from your database using the fileId
           

            if (string.IsNullOrEmpty(fileId))
                return NotFound("File not found.");

            if (Guid.TryParse(fileId, out Guid fileGuiID))
            {
                Console.WriteLine($"Valid GUID: {fileId}");
            }
            else
            {
                return NotFound("Invalid GUID format. {fileId}");
            }

            var record = fileUploadDao.GetUploadById(fileId);

            if (record is null)
            {
                return NotFound("No file associated with file id: {fileId}");
            }
            else
            {
                
                try
                {
                    var s3Object = await s3FileService.GetFileAsync(record.s3_key);
                    var stream = s3Object.ResponseStream;
                    var contentType = s3Object.Headers.ContentType ?? "application/octet-stream";

                    return new FileStreamResult(stream, contentType)
                    {
                        FileDownloadName = record.filename // This will set the Content-Disposition header automatically
                    };
                }
                catch (AmazonS3Exception ex)
                {
                    return BadRequest($"Error retrieving file from S3: {ex.Message}");
                }
            }
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

        [HttpPost("filter")]
        public async Task<IList<FileUpload>> GetFilteredFiles([FromBody] FileFilterRequest filter)
        {
            if (filter == null)
            {
                return null;
            }

            var query = fileUploadDao.getQuery();
            var userquery = userInfoDao.getQuery();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.userid))
            {
                var user = await userInfoDao.GetUserByUsernameAsync(filter.userid);
                if (user.userrole == UserRoles.USER)
                {
                    query = query.Where(f => f.userinfo == filter.userid);
                }
            }

            if (!string.IsNullOrEmpty(filter.byuser))            
            {
                
                query = query.Where(f => f.userinfo == filter.byuser);                
            }

            if (!string.IsNullOrEmpty(filter.year) && !filter.year.ToLower().Contains("all") && int.TryParse(filter.year, out int myear))
                query = query.Where(f => f.year == myear);

            if (!string.IsNullOrEmpty(filter.category) && !filter.category.ToLower().Contains("all"))
                query = query.Where(f => f.category == filter.category);

            if (!string.IsNullOrEmpty(filter.filetype) && !filter.filetype.ToLower().Contains("all"))
                query = query.Where(f => f.filetype == filter.filetype);

            if (!string.IsNullOrEmpty(filter.status) &&
                Enum.TryParse<FileStatus>(filter.status, true, out var fileStatus) && !filter.status.ToLower().Contains("all"))
            {
                query = query.Where(f => f.status == fileStatus.ToString());
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(filter.sortfield) && !string.IsNullOrEmpty(filter.sortorder))
            {
                query = filter.sortorder.ToLower() == "desc"
                    ? query.OrderByDescending(e => EF.Property<object>(e, filter.sortfield))
                    : query.OrderBy(e => EF.Property<object>(e, filter.sortfield));
            }

            var joinedQuery = from f in query
                                  //join u in userquery
                                  //on f.userinfo equals u.username
                              select f;

            var result = await query.ToListAsync();
            return result;
        }

        [HttpDelete("{fileId}")]
        public async Task<IActionResult> DeleteFile(string fileId)
        {
            if (!Guid.TryParse(fileId, out Guid fileGuid))
            {
                return BadRequest("Invalid file ID.");
            }

            var record = fileUploadDao.GetUploadById(fileId);
            if (record == null)
            {
                return NotFound($"File with ID {fileId} not found.");
            }

            try
            {
                // Delete from S3
                await s3FileService.DeleteFileAsync(record.s3_key);

                // Delete from database
                fileUploadDao.DeleteFileUpload(fileGuid);

                return Ok(new { Message = $"File {record.filename} deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = $"Error deleting file: {ex.Message}" });
            }
        }

    }

}

