using System.Text.Json.Serialization;

namespace UCS_Projeto_Integrador_IV_B.Models
{
    public class RankingItem
    {
        [JsonPropertyName("nome")]
        public string Nome { get; set; } = string.Empty;

        [JsonPropertyName("frequencia")]
        public long Frequencia { get; set; }

        [JsonPropertyName("ranking")]
        public int Ranking { get; set; }
    }
}
