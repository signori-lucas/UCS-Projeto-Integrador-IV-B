using Microsoft.AspNetCore.Mvc;
using UCS_Projeto_Integrador_IV_B.Models;

namespace UCS_Projeto_Integrador_IV_B.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class RankingsController : ControllerBase
    {
        private readonly UCS_Projeto_Integrador_IV_B.Services.IIBGERankingService _rankingService;

        public RankingsController(UCS_Projeto_Integrador_IV_B.Services.IIBGERankingService rankingService)
        {
            _rankingService = rankingService;
        }

        // GET: api/rankings?sex=M&state=BR
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RankingResponse>>> Get([FromQuery] string sex = "all", [FromQuery] int stateId = 0, [FromQuery] string decada = "all")
        {
            try
            {
                var result = await _rankingService.GetRankingAsync(sex, stateId);
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

        // GET: api/rankings/by-decade?sex=M&state=BR&decada=1990
        [HttpGet("by-decade")]
        public async Task<ActionResult<IEnumerable<RankingResponse>>> GetByDecade([FromQuery] string decada, [FromQuery] string sex = "all", [FromQuery] int stateId = 0)
        {
            try
            {
                var result = await _rankingService.GetRankingByDecadeAsync(decada, sex, stateId);
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

        [HttpGet("states")]
        public async Task<ActionResult<IEnumerable<StateModel>>> GetStates()
        {
            try
            {
                var states = await _rankingService.GetStatesAsync();
                return Ok(states);
            }
            catch (HttpRequestException hre)
            {
                return StatusCode(502, "Error fetching states from IBGE: " + hre.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
