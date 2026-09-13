using Microsoft.AspNetCore.Mvc;
using UCS_Projeto_Integrador_IV_B.Models;

namespace UCS_Projeto_Integrador_IV_B.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class NamesController : ControllerBase
    {
        private readonly UCS_Projeto_Integrador_IV_B.Services.IIBGERankingService _rankingService;

        public NamesController(UCS_Projeto_Integrador_IV_B.Services.IIBGERankingService rankingService)
        {
            _rankingService = rankingService;
        }

        // GET: api/names/search?name=Maria&stateId=1
        [HttpGet("search")]
        public async Task<ActionResult<NameSearchResult>> Search([FromQuery] string name, [FromQuery] string decada = null, [FromQuery] int stateId = 0)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("name is required");

            try
            {
                var result = await _rankingService.SearchByNameAsync(name.Trim(), decada, stateId);
                if (result == null)
                    return NotFound();

                return Ok(result);
            }
            catch (HttpRequestException hre)
            {
                return StatusCode(502, "Error fetching data from IBGE: " + hre.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
