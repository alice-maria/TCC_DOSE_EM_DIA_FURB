using System.Globalization;
using System.Text.Json;

namespace DoseEmDia.Services.Geo
{
    public sealed class PostoVacinacaoLocService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public PostoVacinacaoLocService(HttpClient http, IConfiguration config)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _http.Timeout = TimeSpan.FromSeconds(10);
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<IReadOnlyList<PostoMaisProximo>> PostosMaisProximos(string enderecoTexto, int raioMetros = 10_000, int limite = 3, CancellationToken ct = default)         
        {
            if (string.IsNullOrWhiteSpace(enderecoTexto))
                throw new ArgumentException("Endereço não pode ser vazio.", nameof(enderecoTexto));

            var (lat, lng) = await GeocodeAsync(enderecoTexto, ct)
                ?? throw new InvalidOperationException("Não foi possível geocodificar o endereço.");

            var candidatos = await PesquisarResultadosAsync(lat, lng, raioMetros, ct);

            foreach (var c in candidatos.Where(c => c.DistanceMeters is null))
            {
                c.DistanceMeters = (int)Math.Round(HaversineMeters(lat, lng, c.Latitude, c.Longitude));
                c.DistanceText = FormatDistance(c.DistanceMeters.Value);
            }

            return candidatos
                .Where(c => c.DistanceMeters.HasValue)
                .OrderBy(c => c.DistanceMeters!.Value)
                .Take(Math.Max(1, limite))
                .ToList();
        }

        private async Task<(double lat, double lng)?> GeocodeAsync(string endereco, CancellationToken ct)
        {
            var apiKey = ObterApiKey();

            var url =
                "https://geocode.search.hereapi.com/v1/geocode" +
                $"?q={Uri.EscapeDataString(endereco)}&in=countryCode:BRA&lang=pt-BR&limit=3&apiKey={apiKey}";

            using var resp = await GetComRetryAsync(url, ct);
            if (!resp.IsSuccessStatusCode) return null;

            var json = await resp.Content.ReadAsStringAsync(); 
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("items", out var items) ||
                items.ValueKind != JsonValueKind.Array || items.GetArrayLength() == 0)
                return null;

            JsonElement? melhor = null;
            var melhorScore = -1;
            foreach (var item in items.EnumerateArray())
            {
                var score = item.TryGetProperty("resultType", out var rt) switch
                {
                    true when rt.GetString() == "houseNumber" => 3,
                    true when rt.GetString() == "street" => 2,
                    true when rt.GetString() == "locality" => 1,
                    _ => 0
                };
                if (score > melhorScore) { melhorScore = score; melhor = item; }
            }

            var escolhido = melhor ?? items.EnumerateArray().First();
            if (!escolhido.TryGetProperty("position", out var pos)) return null;

            if (pos.TryGetProperty("lat", out var latEl) &&
                pos.TryGetProperty("lng", out var lngEl) &&
                latEl.TryGetDouble(out var lat) &&
                lngEl.TryGetDouble(out var lng))
                return (lat, lng);

            return null;
        }

        private async Task<List<PostoMaisProximo>> PesquisarResultadosAsync(double lat, double lng, int raioMetros, CancellationToken ct)
        {
            var apiKey = ObterApiKey();
            var baseCoord = $"{lat.ToString(CultureInfo.InvariantCulture)},{lng.ToString(CultureInfo.InvariantCulture)}";

            var termos = new[] { "Unidade Básica de Saúde", "UBS", "Posto de Saúde", "Unidade de Saúde", "Clínica" };
            var resultados = new List<PostoMaisProximo>();

            foreach (var termo in termos)
            {
                var url =
                    "https://discover.search.hereapi.com/v1/discover" +
                    $"?q={Uri.EscapeDataString(termo)}" +
                    $"&at={baseCoord}" +
                    $"&limit=20&lang=pt-BR&apiKey={apiKey}";
                resultados.AddRange(await FetchBrowsePageAsync(url, ct));
            }

            var categories = "health-care.clinic,health-care.hospital";
            {
                var url =
                    "https://discover.search.hereapi.com/v1/browse" +
                    $"?categories={categories}" +
                    $"&in=circle:{baseCoord};r={raioMetros}" +
                    $"&at={baseCoord}" +
                    $"&limit=20&lang=pt-BR&apiKey={apiKey}";
                resultados.AddRange(await FetchBrowsePageAsync(url, ct));
            }

            return resultados
                .GroupBy(x => $"{(x.Name ?? "").Trim().ToLowerInvariant()}|{x.Latitude:0.000000}|{x.Longitude:0.000000}")
                .Select(g => g.OrderBy(r => r.DistanceMeters ?? int.MaxValue).First())
                .ToList();
        }

        private async Task<List<PostoMaisProximo>> FetchBrowsePageAsync(string url, CancellationToken ct)
        {
            using var resp = await GetComRetryAsync(url, ct);
            if (!resp.IsSuccessStatusCode) return new List<PostoMaisProximo>();

            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("items", out var items) ||
                items.ValueKind != JsonValueKind.Array)
                return new List<PostoMaisProximo>();

            var lista = new List<PostoMaisProximo>();
            foreach (var item in items.EnumerateArray())
            {
                if (!item.TryGetProperty("position", out var pos)) continue;
                if (!pos.TryGetProperty("lat", out var latEl)) continue;
                if (!pos.TryGetProperty("lng", out var lngEl)) continue;
                if (!latEl.TryGetDouble(out var plat) || !lngEl.TryGetDouble(out var plng)) continue;

                var name = item.TryGetProperty("title", out var tEl) ? (tEl.GetString() ?? "") : "";
                var address = ParseAddress(item);

                int? distance = null;
                if (item.TryGetProperty("distance", out var dEl) && dEl.TryGetInt32(out var d))
                    distance = d;

                lista.Add(new PostoMaisProximo
                {
                    Name = string.IsNullOrWhiteSpace(name) ? "Unidade de Saúde" : name.Trim(),
                    Address = address,
                    Latitude = plat,
                    Longitude = plng,
                    DistanceMeters = distance,
                    DistanceText = distance is null ? null : FormatDistance(distance.Value)
                });
            }

            return lista;
        }

        private static string ParseAddress(JsonElement item)
        {
            if (!item.TryGetProperty("address", out var address)) return "";
            string Get(string key) => address.TryGetProperty(key, out var el) ? el.GetString() ?? "" : "";

            var street = Get("street");
            var district = Get("district");
            var city = Get("city");
            var state = Get("state");

            var texto = $"{street}, {district} - {city}/{state}".Trim();
            return texto.TrimStart(',').Replace(" ,", ",").Replace("  ", " ");
        }

        private static string FormatDistance(int meters)
        {
            var km = meters / 1000.0;
            return meters < 1000
                ? $"{meters} m"
                : string.Format(new CultureInfo("pt-BR"), "{0:0.0} km", km);
        }

        private static double HaversineMeters(double lat1, double lon1, double lat2, double lon2)
        {
            static double ToRad(double deg) => Math.PI * deg / 180.0;
            const double R = 6_371_000.0; // metros
            var dLat = ToRad(lat2 - lat1);
            var dLon = ToRad(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private string ObterApiKey()
        {
            var key = _config["Here:ApiKey"];
            if (string.IsNullOrWhiteSpace(key)) key = _config["HERE:ApiKey"];
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("Chave da HERE API não configurada (Here:ApiKey ou HERE:ApiKey).");
            return key;
        }

        private async Task<HttpResponseMessage> GetComRetryAsync(string url, CancellationToken ct)
        {
            const int maxTentativas = 3;

            for (var i = 1; i <= maxTentativas; i++)
            {
                try
                {
                    var resp = await _http.GetAsync(url, ct);

                    if ((int)resp.StatusCode == 429)
                    {
                        if (i == maxTentativas) return resp;
                        var wait = resp.Headers.RetryAfter?.Delta ?? TimeSpan.FromMilliseconds(200 * i);
                        resp.Dispose();
                        await Task.Delay(wait, ct);
                        continue;
                    }

                    if ((int)resp.StatusCode >= 500)
                    {
                        if (i == maxTentativas) return resp;
                        resp.Dispose();
                        await Task.Delay(200 * i, ct);
                        continue;
                    }

                    return resp;
                }
                catch (TaskCanceledException) when (!ct.IsCancellationRequested)
                {
                    if (i == maxTentativas) throw;
                    await Task.Delay(200 * i, ct);
                }
                catch (HttpRequestException)
                {
                    if (i == maxTentativas) throw;
                    await Task.Delay(200 * i, ct);
                }
            }

            throw new InvalidOperationException("Falha inesperada no retry HTTP.");
        }
    }

    public sealed class PostoMaisProximo
    {
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int? DistanceMeters { get; set; }
        public string? DistanceText { get; set; }
    }
}
