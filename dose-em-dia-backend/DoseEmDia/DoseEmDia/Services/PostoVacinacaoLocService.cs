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

        public async Task<IReadOnlyList<PostoMaisProximo>> ObterPostosMaisProximos(string enderecoTexto, int raioMetros = 10_000, int limite = 3, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(enderecoTexto))
                throw new ArgumentException("Endereço não pode ser vazio.", nameof(enderecoTexto));

            var (lat, lng) = await GeocodificarAsync(enderecoTexto, ct)
                ?? throw new InvalidOperationException("Não foi possível geocodificar o endereço.");

            var candidatos = await PesquisarPostosAsync(lat, lng, raioMetros, ct);

            foreach (var c in candidatos.Where(c => c.DistanciaMetros is null))
            {
                c.DistanciaMetros = (int)Math.Round(CalcularHaversineMetros(lat, lng, c.Latitude, c.Longitude));
                c.TextoDistancia = FormatarDistancia(c.DistanciaMetros.Value);
            }

            return candidatos
                .Where(c => c.DistanciaMetros.HasValue)
                .OrderBy(c => c.DistanciaMetros!.Value)
                .Take(Math.Max(1, limite))
                .ToList();
        }

        private async Task<(double lat, double lng)?> GeocodificarAsync(string endereco, CancellationToken ct)
        {
            var chaveApi = ObterChaveApi();

            var url =
                "https://geocode.search.hereapi.com/v1/geocode" +
                $"?q={Uri.EscapeDataString(endereco)}&in=countryCode:BRA&lang=pt-BR&limit=3&apiKey={chaveApi}";

            using var resp = await RequisicaoComRetryAsync(url, ct);
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

        private async Task<List<PostoMaisProximo>> PesquisarPostosAsync(double lat, double lng, int raioMetros, CancellationToken ct)
        {
            var chaveApi = ObterChaveApi();
            var coordenadaBase = $"{lat.ToString(CultureInfo.InvariantCulture)},{lng.ToString(CultureInfo.InvariantCulture)}";

            var termos = new[] { "Unidade Básica de Saúde", "UBS", "Posto de Saúde", "Unidade de Saúde", "Clínica" };
            var resultados = new List<PostoMaisProximo>();

            foreach (var termo in termos)
            {
                var url =
                    "https://discover.search.hereapi.com/v1/discover" +
                    $"?q={Uri.EscapeDataString(termo)}" +
                    $"&at={coordenadaBase}" +
                    $"&limit=20&lang=pt-BR&apiKey={chaveApi}";
                resultados.AddRange(await BuscarPaginaAsync(url, ct));
            }

            var categorias = "health-care.clinic,health-care.hospital";
            {
                var url =
                    "https://discover.search.hereapi.com/v1/browse" +
                    $"?categories={categorias}" +
                    $"&in=circle:{coordenadaBase};r={raioMetros}" +
                    $"&at={coordenadaBase}" +
                    $"&limit=20&lang=pt-BR&apiKey={chaveApi}";
                resultados.AddRange(await BuscarPaginaAsync(url, ct));
            }

            return resultados
                .GroupBy(x => $"{(x.Nome ?? "").Trim().ToLowerInvariant()}|{x.Latitude:0.000000}|{x.Longitude:0.000000}")
                .Select(g => g.OrderBy(r => r.DistanciaMetros ?? int.MaxValue).First())
                .ToList();
        }

        private async Task<List<PostoMaisProximo>> BuscarPaginaAsync(string url, CancellationToken ct)
        {
            using var resp = await RequisicaoComRetryAsync(url, ct);
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

                var nome = item.TryGetProperty("title", out var tEl) ? (tEl.GetString() ?? "") : "";
                var endereco = ExtrairEndereco(item);

                int? distancia = null;
                if (item.TryGetProperty("distance", out var dEl) && dEl.TryGetInt32(out var d))
                    distancia = d;

                lista.Add(new PostoMaisProximo
                {
                    Nome = string.IsNullOrWhiteSpace(nome) ? "Unidade de Saúde" : nome.Trim(),
                    Endereco = endereco,
                    Latitude = plat,
                    Longitude = plng,
                    DistanciaMetros = distancia,
                    TextoDistancia = distancia is null ? null : FormatarDistancia(distancia.Value)
                });
            }

            return lista;
        }

        private static string ExtrairEndereco(JsonElement item)
        {
            if (!item.TryGetProperty("address", out var address)) return "";
            string Obter(string chave) => address.TryGetProperty(chave, out var el) ? el.GetString() ?? "" : "";

            var rua = Obter("street");
            var bairro = Obter("district");
            var cidade = Obter("city");
            var estado = Obter("state");

            var texto = $"{rua}, {bairro} - {cidade}/{estado}".Trim();
            return texto.TrimStart(',').Replace(" ,", ",").Replace("  ", " ");
        }

        private static string FormatarDistancia(int metros)
        {
            var km = metros / 1000.0;
            return metros < 1000
                ? $"{metros} m"
                : string.Format(new CultureInfo("pt-BR"), "{0:0.0} km", km);
        }

        private static double CalcularHaversineMetros(double lat1, double lon1, double lat2, double lon2)
        {
            static double ParaRad(double deg) => Math.PI * deg / 180.0;
            const double R = 6_371_000.0; // metros
            var dLat = ParaRad(lat2 - lat1);
            var dLon = ParaRad(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ParaRad(lat1)) * Math.Cos(ParaRad(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private string ObterChaveApi()
        {
            var chave = _config["Here:ApiKey"];
            if (string.IsNullOrWhiteSpace(chave)) chave = _config["HERE:ApiKey"];
            if (string.IsNullOrWhiteSpace(chave))
                throw new InvalidOperationException("Chave da HERE API não configurada (Here:ApiKey ou HERE:ApiKey).");
            return chave;
        }

        private async Task<HttpResponseMessage> RequisicaoComRetryAsync(string url, CancellationToken ct)
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
                        var espera = resp.Headers.RetryAfter?.Delta ?? TimeSpan.FromMilliseconds(200 * i);
                        resp.Dispose();
                        await Task.Delay(espera, ct);
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
        public string Nome { get; set; } = "";
        public string Endereco { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int? DistanciaMetros { get; set; }
        public string? TextoDistancia { get; set; }
    }
}