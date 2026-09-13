using Microsoft.AspNetCore.Mvc;
using UCS_Projeto_Integrador_IV_B.Models;

namespace UCS_Projeto_Integrador_IV_B.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class NamesController : ControllerBase
    {
        // GET: api/names/search?name=Maria&state=BR
        [HttpGet("search")]
        public ActionResult<NameSearchResult> Search([FromQuery] string name, [FromQuery] string state = "BR")
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("name is required");

            // Mocked response - in real implementation call IBGE names API
            var normalized = name.Trim();
            var rnd = new Random(normalized.GetHashCode() ^ state.GetHashCode());
            var total = Math.Abs(rnd.Next(1000, 20000000));
            var freq = Math.Round((double)(total % 1000) / 1000.0, 4);

            var result = new NameSearchResult
            {
                Name = normalized,
                Total = total,
                Frequency = freq
            };

            return Ok(result);
        }
    }
}
