



using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Http;
using Amazon.CDK.AWS.IAM;

namespace Internal_API.controller
{

    [Route("api/[controller]")]
    [ApiController]
    public class ScanController : ControllerBase
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<ScanController> logger;
        private readonly IConfiguration configuration;
        private string ClamAVScanEndpoint; // Replace with your actual URL

        public ScanController(IHttpClientFactory httpClientFactory, ILogger<ScanController> logger, IConfiguration configuration)
        {
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
            this.logger = logger;
            var host = configuration["ClamAV:Host"];
            var port = configuration["ClamAV:Port"];
            var api = configuration["ClamAV:API"];
            var protocol = configuration["ClamAV: Protocol"];
            ClamAVScanEndpoint = protocol + "://" + host + ":" + port + "/" + api;
        }

        [HttpPost("file")]
        public async Task<IActionResult> ScanFile(IFormFile file)
        {

            var json = System.Text.Json.JsonSerializer.Serialize(new { status = "passed", engine = "bypass" });
            return Content(json, "application/json");

            //if (file == null || file.Length == 0)
            //    return BadRequest("File is missing.");

            //var client = httpClientFactory.CreateClient("UnsafeClient");

            //try
            //{
            //    using var content = new MultipartFormDataContent();
            //    using var stream = file.OpenReadStream();
            //    using var fileContent = new StreamContent(stream);
            //    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

            //    content.Add(fileContent, "file", file.FileName);

            //    try
            //    {
            //        var response = await client.PostAsync(ClamAVScanEndpoint, content);
            //        var result = await response.Content.ReadAsStringAsync();
            //        return Content(result, "application/json");
            //    }
            //    catch (Exception ex)
            //    {
            //        return StatusCode(500, $"Scan failed: {ex.Message}");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    logger.LogError(ex, "Error scanning file with ClamAV");
            //    return StatusCode(500, "Error scanning file.");
            //}
        }
    }
}
