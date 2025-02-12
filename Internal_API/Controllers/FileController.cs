using Microsoft.AspNetCore.Mvc;
using Internal_API.Services;
using Internal_API.models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Internal_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class S3FileController : ControllerBase
    {
        private readonly IS3Service _s3FileService;

        public S3FileController(IS3Service s3FileService)
        {
            _s3FileService = s3FileService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] S3FileInfo fileInfo)
        {
            try
            {
                var result = await _s3FileService.UploadFileAsync(fileInfo);
                return Ok(new { Message = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}
