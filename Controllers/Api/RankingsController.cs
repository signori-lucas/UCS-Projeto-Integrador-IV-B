using Microsoft.AspNetCore.Mvc;
using UCS_Projeto_Integrador_IV_B.Models;

namespace UCS_Projeto_Integrador_IV_B.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class RankingsController : ControllerBase
    {
        // GET: api/rankings?sex=M&state=BR&decada=1990
        [HttpGet]
        public ActionResult<IEnumerable<NameRanking>> Get([FromQuery] string sex = "all", [FromQuery] string state = "BR", [FromQuery] string decada = "all")
        {
            // Mock data - in future replace with calls to IBGE APIs
            var all = new List<NameRanking>
            {
                new NameRanking{ Rank=1, Name="Maria", Total=13500000 },
                new NameRanking{ Rank=2, Name="Ana", Total=12500000 },
                new NameRanking{ Rank=3, Name="João", Total=12000000 },
                new NameRanking{ Rank=4, Name="Francisco", Total=11500000 },
                new NameRanking{ Rank=5, Name="Antonio", Total=11000000 },
                new NameRanking{ Rank=6, Name="Carlos", Total=10500000 },
                new NameRanking{ Rank=7, Name="Paulo", Total=10000000 },
                new NameRanking{ Rank=8, Name="José", Total=9500000 },
                new NameRanking{ Rank=9, Name="Mariana", Total=9000000 },
                new NameRanking{ Rank=10, Name="Beatriz", Total=8500000 }
            };

            // Very simple filtering by sex simulated by name list
            if (!string.Equals(sex, "all", StringComparison.OrdinalIgnoreCase))
            {
                if (sex.Equals("M", StringComparison.OrdinalIgnoreCase))
                {
                    all = all.Where(x => new[] { "João","Francisco","Antonio","Carlos","Paulo","José" }.Contains(x.Name)).ToList();
                }
                else if (sex.Equals("F", StringComparison.OrdinalIgnoreCase))
                {
                    all = all.Where(x => new[] { "Maria","Ana","Mariana","Beatriz" }.Contains(x.Name)).ToList();
                }
            }

            // Simulate decade effect by applying a multiplier to totals
            var multipliers = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                { "1930", 0.3 }, { "1940", 0.4 }, { "1950", 0.5 }, { "1960", 0.6 }, { "1970", 0.7 },
                { "1980", 0.8 }, { "1990", 0.9 }, { "2000", 0.95 }, { "2010", 1.0 }, { "all", 1.0 }
            };

            var key = string.IsNullOrWhiteSpace(decada) ? "all" : decada;
            if (!multipliers.TryGetValue(key, out var mult))
                mult = 1.0;

            foreach (var item in all)
            {
                item.Total = (long)Math.Round(item.Total * mult);
            }

            // State filter currently ignored for mock; preserve parameter for future

            return Ok(all.Take(10));
        }

        // GET: api/rankings/by-decade?sex=M&state=BR&decada=1990
        [HttpGet("by-decade")]
        public ActionResult<IEnumerable<NameRanking>> GetByDecade([FromQuery] string decada, [FromQuery] string sex = "all", [FromQuery] string state = "BR")
        {
            // Base dataset
            var all = new List<NameRanking>
            {
                new NameRanking{ Rank=0, Name="Maria", Total=13500000 },
                new NameRanking{ Rank=0, Name="Ana", Total=12500000 },
                new NameRanking{ Rank=0, Name="João", Total=12000000 },
                new NameRanking{ Rank=0, Name="Francisco", Total=11500000 },
                new NameRanking{ Rank=0, Name="Antonio", Total=11000000 },
                new NameRanking{ Rank=0, Name="Carlos", Total=10500000 },
                new NameRanking{ Rank=0, Name="Paulo", Total=10000000 },
                new NameRanking{ Rank=0, Name="José", Total=9500000 },
                new NameRanking{ Rank=0, Name="Mariana", Total=9000000 },
                new NameRanking{ Rank=0, Name="Beatriz", Total=8500000 }
            };

            // Filter by sex (mock)
            if (!string.Equals(sex, "all", StringComparison.OrdinalIgnoreCase))
            {
                if (sex.Equals("M", StringComparison.OrdinalIgnoreCase))
                {
                    all = all.Where(x => new[] { "João","Francisco","Antonio","Carlos","Paulo","José" }.Contains(x.Name)).ToList();
                }
                else if (sex.Equals("F", StringComparison.OrdinalIgnoreCase))
                {
                    all = all.Where(x => new[] { "Maria","Ana","Mariana","Beatriz" }.Contains(x.Name)).ToList();
                }
            }

            var multipliers = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                { "1930", 0.3 }, { "1940", 0.4 }, { "1950", 0.5 }, { "1960", 0.6 }, { "1970", 0.7 },
                { "1980", 0.8 }, { "1990", 0.9 }, { "2000", 0.95 }, { "2010", 1.0 }
            };

            var key = string.IsNullOrWhiteSpace(decada) ? "1990" : decada;
            if (!multipliers.TryGetValue(key, out var mult)) mult = 1.0;

            // Apply multiplier and order by total desc
            var transformed = all.Select(x => new NameRanking { Name = x.Name, Total = (long)Math.Round(x.Total * mult) })
                                 .OrderByDescending(x => x.Total)
                                 .Take(10)
                                 .Select((x, idx) => { x.Rank = idx + 1; return x; })
                                 .ToList();

            return Ok(transformed);
        }

        [HttpGet("states")]
        public ActionResult<IEnumerable<StateModel>> GetStates()
        {
            var states = new List<StateModel>
            {
                new StateModel{ Code = "BR", Name = "Brasil" },
                new StateModel{ Code = "AC", Name = "Acre" },
                new StateModel{ Code = "AL", Name = "Alagoas" },
                new StateModel{ Code = "AP", Name = "Amapá" },
                new StateModel{ Code = "AM", Name = "Amazonas" },
                new StateModel{ Code = "BA", Name = "Bahia" },
                new StateModel{ Code = "CE", Name = "Ceará" },
                new StateModel{ Code = "DF", Name = "Distrito Federal" },
                new StateModel{ Code = "ES", Name = "Espírito Santo" },
                new StateModel{ Code = "GO", Name = "Goiás" },
                new StateModel{ Code = "MA", Name = "Maranhão" },
                new StateModel{ Code = "MT", Name = "Mato Grosso" },
                new StateModel{ Code = "MS", Name = "Mato Grosso do Sul" },
                new StateModel{ Code = "MG", Name = "Minas Gerais" },
                new StateModel{ Code = "PA", Name = "Pará" },
                new StateModel{ Code = "PB", Name = "Paraíba" },
                new StateModel{ Code = "PR", Name = "Paraná" },
                new StateModel{ Code = "PE", Name = "Pernambuco" },
                new StateModel{ Code = "PI", Name = "Piauí" },
                new StateModel{ Code = "RJ", Name = "Rio de Janeiro" },
                new StateModel{ Code = "RN", Name = "Rio Grande do Norte" },
                new StateModel{ Code = "RS", Name = "Rio Grande do Sul" },
                new StateModel{ Code = "RO", Name = "Rondônia" },
                new StateModel{ Code = "RR", Name = "Roraima" },
                new StateModel{ Code = "SC", Name = "Santa Catarina" },
                new StateModel{ Code = "SP", Name = "São Paulo" },
                new StateModel{ Code = "SE", Name = "Sergipe" },
                new StateModel{ Code = "TO", Name = "Tocantins" }
            };

            return Ok(states);
        }
    }
}
