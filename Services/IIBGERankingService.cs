using System.Collections.Generic;
using System.Threading.Tasks;
using UCS_Projeto_Integrador_IV_B.Models;

namespace UCS_Projeto_Integrador_IV_B.Services
{
    public interface IIBGERankingService
    {
        Task<IEnumerable<RankingResponse>> GetRankingAsync(string sex = "all", int stateId = 0);
        Task<IEnumerable<RankingResponse>> GetRankingByDecadeAsync(string decada, string sex = "all", int stateId = 0);
        Task<NameSearchResult?> SearchByNameAsync(string name, string decada = null, int stateId = 0);
        Task<IEnumerable<UCS_Projeto_Integrador_IV_B.Models.StateModel>> GetStatesAsync();
    }
}
