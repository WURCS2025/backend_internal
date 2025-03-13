


using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Internal_API.controller
{



    namespace ClamAVApi.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        public class ScanController : ControllerBase
        {
            private readonly HttpClient httpClient;
            private readonly IConfiguration configuration;
            private string ClamAV_URL;

            public ScanController(IConfiguration configuration)
            {
                httpClient = new HttpClient();
                this.configuration = configuration;
                var ClamAV_host = configuration["ClamAV:Host"];
                var ClamAV_port = configuration["ClamAV:Port"];
                var ClamAV_Api = configuration["ClamAV:API"];
                ClamAV_URL = "http://" + ClamAV_host + ":" + ClamAV_port + "/" + ClamAV_Api;
            }

            [HttpPost("scanfile")]
            public async Task<IActionResult> ScanFile(IFormFile file)
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded.");
                }

                using (var stream = file.OpenReadStream())
                using (var content = new StreamContent(stream))
                {
                    // Send the file to the ClamAV Docker container
                    var response = await httpClient.PostAsync(ClamAV_URL, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        return Ok(new { Message = "Scan completed.", Result = result });
                    }
                    else
                    {
                        return StatusCode((int)response.StatusCode, new { Message = "Scan failed.", Error = response.ReasonPhrase });
                    }
                }
            }
        }
    }

}
