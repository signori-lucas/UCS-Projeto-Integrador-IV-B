using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace UCS_Projeto_Integrador_IV_B.Models
{
    public class RankingResponse
    {
        [JsonPropertyName("localidade")]
        public string Localidade { get; set; } = string.Empty;

        [JsonPropertyName("sexo")]
        public string Sexo { get; set; } = string.Empty;

        [JsonPropertyName("res")]
        public List<RankingItem> Res { get; set; } = new List<RankingItem>();
    }
}
