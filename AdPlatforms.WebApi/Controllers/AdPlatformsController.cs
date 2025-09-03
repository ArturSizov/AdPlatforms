using AdPlatforms.Domain.UseCases.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace AdPlatforms.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdPlatformsController(ILogger<AdPlatformsController> logger, IAdPlatformSelectorUseCase selector) : ControllerBase
    {
        [HttpPost("upload")]
        public async Task<IActionResult> UploadPlatformsListAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            if (file.Length == 0)
                return BadRequest("Empty file");

            try
            {
                // Open the file stream
                using var stream = file.OpenReadStream();

                // Load data using the use case
                var (success, error) = await selector.LoadDataAsync(stream, cancellationToken);

                // Respond based on the result
                return success
                    ? Ok("File processed successfully")
                    : BadRequest(error);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error processing file");
                return StatusCode(500, "Unexpected error processing file");
            }
        }

        [HttpPost("retrieve")]
        public async Task<IActionResult> GetPlatformsAsync(string location, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(location))
                return Ok(Array.Empty<string>());

            var (platforms, error) = await selector.GetPlatformsAsync(location, cancellationToken);
            if (error != null)
                return BadRequest(error);

            return Ok(platforms);
        }
    }

}
