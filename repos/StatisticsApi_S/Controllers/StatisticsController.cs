using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using StatisticsApi_S.Model;
using StatisticsApi_S.Service;

namespace StatisticsApi_S.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    //[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticService _service;

        public StatisticsController(IStatisticService service)
        {
            _service = service;
        }


        [HttpPost("statistics")]
        [Consumes("multipart/form-data")]
        //[Produces("application/octet-stream")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<Model.StatisticModel>>> Statistics(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("Missing parquet file.");
                List<Model.StatisticModel> statisticModels = new List<Model.StatisticModel>();
                using var stream = file.OpenReadStream();

                var result = await _service.GetCalculatedStatisticAsync(stream);

                if (result == null)
                {
                    return BadRequest("Error processing the parquet file.");
                }
                else
                {

                    return Ok(result);
                }
                //return File(
                //result,
                //"application/octet-stream",
                //"statistics.parquet");

            }
            catch (Exception ex)
            {
                return BadRequest($"Error processing the parquet file: {ex.Message}");
            }

        }

    }
}
