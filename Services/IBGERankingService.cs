using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using UCS_Projeto_Integrador_IV_B.Models;
using Microsoft.Extensions.Logging;

namespace UCS_Projeto_Integrador_IV_B.Services
{
    public class IBGERankingService : IIBGERankingService
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly ILogger<IBGERankingService> _logger;
        private List<StateModel>? _statesCache;

        public IBGERankingService(IHttpClientFactory httpFactory, ILogger<IBGERankingService> logger)
        {
            _httpFactory = httpFactory;
            _logger = logger;
        }

        private async Task EnsureStatesLoadedAsync()
        {
            if (_statesCache != null) return;
            _statesCache = (await GetStatesAsync()).ToList();
        }

        private async Task<string?> ResolveStateSiglaAsync(int stateId)
        {
            // stateId == 0 means "no filter"; do not provide a localidade parameter
            if (stateId == 0) return null;
            await EnsureStatesLoadedAsync();
            var s = _statesCache?.FirstOrDefault(x => x.Id == stateId);
            return s?.Sigla ?? null;
        }

        public async Task<IEnumerable<RankingResponse>> GetRankingAsync(string sex = "all", int stateId = 0)
        {
            var client = _httpFactory.CreateClient("ibge");
            var url = "api/v2/censos/nomes/ranking";
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(sex) && !sex.Equals("all", StringComparison.OrdinalIgnoreCase)) query.Add($"sexo={Uri.EscapeDataString(sex)}");
            if (stateId != 0) query.Add($"localidade={stateId}");
            if (query.Count > 0) url += "?" + string.Join("&", query);

            _logger?.LogDebug("IBGE GET {Url}", url);
            using var resp = await client.GetAsync(url);
            resp.EnsureSuccessStatusCode();

            using var stream = await resp.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);
            var root = doc.RootElement;

            var list = new List<NameRanking>();

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in root.EnumerateArray())
                {
                    if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty("res", out var resProp) && resProp.ValueKind == JsonValueKind.Array)
                    {
                        int rank = 1;
                        foreach (var item in resProp.EnumerateArray())
                        {
                            var name = item.GetProperty("nome").GetString() ?? string.Empty;
                            long total = 0;
                            if (item.TryGetProperty("frequencia", out var f) && f.ValueKind == JsonValueKind.Number)
                                total = f.GetInt64();

                            list.Add(new NameRanking { Rank = rank++, Name = name, Total = total });
                        }
                        break;
                    }
                }

                if (list.Count == 0)
                {
                    int rank = 1;
                    foreach (var item in root.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.Object && item.TryGetProperty("nome", out var n))
                        {
                            var name = n.GetString() ?? string.Empty;
                            long total = 0;
                            if (item.TryGetProperty("frequencia", out var f) && f.ValueKind == JsonValueKind.Number)
                                total = f.GetInt64();

                            list.Add(new NameRanking { Rank = rank++, Name = name, Total = total });
                        }
                    }
                }
            }
            else if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("res", out var resProp) && resProp.ValueKind == JsonValueKind.Array)
                {
                    int rank = 1;
                    foreach (var item in resProp.EnumerateArray())
                    {
                        var name = item.GetProperty("nome").GetString() ?? string.Empty;
                        long total = 0;
                        if (item.TryGetProperty("frequencia", out var f) && f.ValueKind == JsonValueKind.Number)
                            total = f.GetInt64();

                        list.Add(new NameRanking { Rank = rank++, Name = name, Total = total });
                    }
                }
            }

            var items = list.Take(10).Select(x => new RankingItem
            {
                Nome = x.Name,
                Frequencia = x.Total,
                Ranking = x.Rank
            }).ToList();

            var response = new RankingResponse
            {
                // prefer sigla for display when available
                Localidade = (await ResolveStateSiglaAsync(stateId)) ?? (stateId != 0 ? stateId.ToString() : string.Empty),
                Sexo = sex ?? "all",
                Res = items
            };

            return new[] { response };
        }

        public async Task<NameSearchResult?> SearchByNameAsync(string name, string decada = null, int stateId = 0)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            var client = _httpFactory.CreateClient("ibge");
            // Use the name-specific endpoint: /api/v2/censos/nomes/{name}
            var nameEsc = Uri.EscapeDataString(name);
            var url = $"api/v2/censos/nomes/{nameEsc}";
            var queryParts = new List<string>();
            var sigla = await ResolveStateSiglaAsync(stateId);
            if (!string.IsNullOrWhiteSpace(sigla)) queryParts.Add($"localidade={Uri.EscapeDataString(sigla)}");
            if (queryParts.Count > 0) url += "?" + string.Join("&", queryParts);

            using var resp = await client.GetAsync(url);
            resp.EnsureSuccessStatusCode();

            using var stream = await resp.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);
            var root = doc.RootElement;

            if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
                return null;

            var entry = root[0];
            var resProp = entry.GetProperty("res");
            if (resProp.ValueKind != JsonValueKind.Array)
                return null;

            // determine target decade numeric value, e.g., "1990" -> 1990
            int? targetDec = null;
            if (!string.IsNullOrWhiteSpace(decada) && int.TryParse(decada, out var d)) targetDec = d;

            long foundTotal = 0;
            foreach (var item in resProp.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object) continue;
                if (!item.TryGetProperty("periodo", out var periodoProp)) continue;
                var periodo = periodoProp.GetString() ?? string.Empty;
                // extract first 4-digit year
                var yearStr = System.Text.RegularExpressions.Regex.Match(periodo, "\\d{4}").Value;
                if (string.IsNullOrEmpty(yearStr)) continue;
                if (targetDec.HasValue)
                {
                    if (int.TryParse(yearStr, out var year) && year == targetDec.Value)
                    {
                        if (item.TryGetProperty("frequencia", out var f) && f.ValueKind == JsonValueKind.Number)
                            foundTotal = f.GetInt64();
                        break;
                    }
                }
            }

            if (foundTotal == 0)
                return null;

            var result = new NameSearchResult
            {
                Name = entry.GetProperty("nome").GetString() ?? name,
                Total = foundTotal,
                Frequency = Math.Round((double)foundTotal / 1_000_000.0, 6)
            };

            return result;
        }

        public async Task<IEnumerable<UCS_Projeto_Integrador_IV_B.Models.StateModel>> GetStatesAsync()
        {
            var client = _httpFactory.CreateClient("ibge");
            var url = "api/v1/localidades/estados?orderBy=nome";
            using var resp = await client.GetAsync(url);
            resp.EnsureSuccessStatusCode();

            using var stream = await resp.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);
            var root = doc.RootElement;

            var list = new List<StateModel>();
            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in root.EnumerateArray())
                {
                    var id = item.GetProperty("id").GetInt32();
                    var nome = item.GetProperty("nome").GetString() ?? string.Empty;
                    var sigla = item.TryGetProperty("sigla", out var s) ? s.GetString() ?? string.Empty : string.Empty;
                    list.Add(new StateModel { Id = id, Sigla = sigla, Name = nome });
                }
            }

            return list;
        }

        public async Task<IEnumerable<RankingResponse>> GetRankingByDecadeAsync(string decada, string sex = "all", int stateId = 0)
        {
            var client = _httpFactory.CreateClient("ibge");
            var url = "api/v2/censos/nomes/ranking";
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(decada)) query.Add($"decada={Uri.EscapeDataString(decada)}");
            if (!string.IsNullOrWhiteSpace(sex) && !sex.Equals("all", StringComparison.OrdinalIgnoreCase)) query.Add($"sexo={Uri.EscapeDataString(sex)}");
            // Use numeric IBGE state id for the localidade parameter (some endpoints expect the numeric id)
            if (stateId != 0) query.Add($"localidade={stateId}");
            if (query.Count > 0) url += "?" + string.Join("&", query);

            using var resp = await client.GetAsync(url);
            resp.EnsureSuccessStatusCode();

            using var stream = await resp.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);
            var root = doc.RootElement;

            var list = new List<NameRanking>();

            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var element in root.EnumerateArray())
                {
                    if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty("res", out var resProp) && resProp.ValueKind == JsonValueKind.Array)
                    {
                        int rank = 1;
                        foreach (var item in resProp.EnumerateArray())
                        {
                            var name = item.GetProperty("nome").GetString() ?? string.Empty;
                            long total = 0;
                            if (item.TryGetProperty("frequencia", out var f) && f.ValueKind == JsonValueKind.Number)
                                total = f.GetInt64();

                            list.Add(new NameRanking { Rank = rank++, Name = name, Total = total });
                        }
                        break;
                    }
                }

                if (list.Count == 0)
                {
                    int rank = 1;
                    foreach (var item in root.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.Object && item.TryGetProperty("nome", out var n))
                        {
                            var name = n.GetString() ?? string.Empty;
                            long total = 0;
                            if (item.TryGetProperty("frequencia", out var f) && f.ValueKind == JsonValueKind.Number)
                                total = f.GetInt64();

                            list.Add(new NameRanking { Rank = rank++, Name = name, Total = total });
                        }
                    }
                }
            }
            else if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("res", out var resProp) && resProp.ValueKind == JsonValueKind.Array)
                {
                    int rank = 1;
                    foreach (var item in resProp.EnumerateArray())
                    {
                        var name = item.GetProperty("nome").GetString() ?? string.Empty;
                        long total = 0;
                        if (item.TryGetProperty("frequencia", out var f) && f.ValueKind == JsonValueKind.Number)
                            total = f.GetInt64();

                        list.Add(new NameRanking { Rank = rank++, Name = name, Total = total });
                    }
                }
            }

            var items = list.Take(10).Select(x => new RankingItem
            {
                Nome = x.Name,
                Frequencia = x.Total,
                Ranking = x.Rank
            }).ToList();

            var response = new RankingResponse
            {
                Localidade = (await ResolveStateSiglaAsync(stateId)) ?? (stateId != 0 ? stateId.ToString() : string.Empty),
                Sexo = sex ?? "all",
                Res = items
            };

            return new[] { response };
        }
    }
}
