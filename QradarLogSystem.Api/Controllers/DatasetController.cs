using Microsoft.AspNetCore.Mvc;
using QradarLogSystem.Api.Services;

namespace QradarLogSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatasetController : ControllerBase
    {
        private readonly DatasetImportService _datasetImportService;

        public DatasetController(
            DatasetImportService datasetImportService)
        {
            _datasetImportService = datasetImportService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadDataset(
            IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new
                {
                    message = "Dataset file is empty or missing."
                });
            }

            try
            {
                using var reader =
                    new StreamReader(file.OpenReadStream());

                var content =
                    await reader.ReadToEndAsync();

                var result =
                    _datasetImportService.Import(
                        content,
                        file.FileName);

                return Ok(new
                {
                    message =
                        "Dataset processed successfully.",
                    result
                });
            }
            catch (NotSupportedException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (FormatException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message =
                            "Dataset could not be processed.",
                        error = ex.Message
                    });
            }
        }
    }
}