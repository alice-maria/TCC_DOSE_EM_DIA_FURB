using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

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

        /// <summary>
        /// Fluxo 2 etapas: (1) geocodifica o endereço → lat/lng; (2) busca UBS/Clínicas próximas e retorna ordenado por distância.
        /// </summary>
        public async Task<IReadOnlyList<NearPlaceResult>> BuscarPostosMaisProximosAsync(
            string enderecoTexto,
            int raioMetros = 10_000,
            int limite = 3,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(enderecoTexto))
                throw new ArgumentException("Endereço não pode ser vazio.", nameof(enderecoTexto));

            var (lat, lng) = await GeocodeAsync(enderecoTexto, ct)
                ?? throw new InvalidOperationException("Não foi possível geocodificar o endereço.");

            var candidatos = await BrowseHealthAsync(lat, lng, raioMetros, ct);

            // Se a HERE não fornecer distance, calcule por Haversine.
            foreach (var c in candidatos.Where(c => c.DistanceMeters is null))
                c.DistanceMeters = (int)Math.Round(HaversineMeters(lat, lng, c.Latitude, c.Longitude));

            return candidatos
                .Where(c => c.DistanceMeters.HasValue)
                .OrderBy(c => c.DistanceMeters!.Value)
                .Take(Math.Max(1, limite))
                .ToList();
        }

        // ---------- PASSO 1: GEOCODING ----------
        private async Task<(double lat, double lng)?> GeocodeAsync(string endereco, CancellationToken ct)
        {
            var apiKey = ObterApiKey();
            var url =
                "https://geocode.search.hereapi.com/v1/geocode" +
                $"?q={Uri.EscapeDataString(endereco)}&in=countryCode:BRA&lang=pt-BR&limit=3&apiKey={apiKey}";

            using var resp = await GetComRetryAsync(url, ct);
            if (!resp.IsSuccessStatusCode) return null;

            var json = await resp.Content.ReadAsStringAsync(); // sem CT (compatível c/ várias versões)
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

            JsonElement escolhido = melhor ?? items.EnumerateArray().First();
            if (!escolhido.TryGetProperty("position", out var pos)) return null;

            if (pos.TryGetProperty("lat", out var latEl) &&
                pos.TryGetProperty("lng", out var lngEl) &&
                latEl.TryGetDouble(out var lat) &&
                lngEl.TryGetDouble(out var lng))
                return (lat, lng);

            return null;
        }

        // ---------- PASSO 2: BROWSE ----------
        private async Task<List<NearPlaceResult>> BrowseHealthAsync(double lat, double lng, int raioMetros, CancellationToken ct)
        {
            var apiKey = ObterApiKey();
            var baseCoord = $"{lat.ToString(CultureInfo.InvariantCulture)},{lng.ToString(CultureInfo.InvariantCulture)}";
            var termos = new[] { "Unidade Básica de Saúde", "UBS", "Posto de Saúde", "Unidade de Saúde", "Clínica" };

            var resultados = new List<NearPlaceResult>();

            // 2a) termos texto
            foreach (var termo in termos)
            {
                var url =
                    "https://discover.search.hereapi.com/v1/browse" +
                    $"?q={Uri.EscapeDataString(termo)}" +
                    $"&in=circle:{baseCoord};r={raioMetros}" +
                    $"&at={baseCoord}" +
                    $"&limit=20&lang=pt-BR&apiKey={apiKey}";

                resultados.AddRange(await FetchBrowsePageAsync(url, ct));
            }

            // 2b) categorias (fallback)
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

            // Dedup por (nome + lat + lng)
            return resultados
                .GroupBy(x => $"{x.Name}|{x.Latitude:0.000000}|{x.Longitude:0.000000}")
                .Select(g => g.OrderBy(r => r.DistanceMeters ?? int.MaxValue).First())
                .ToList();
        }

        private async Task<List<NearPlaceResult>> FetchBrowsePageAsync(string url, CancellationToken ct)
        {
            using var resp = await GetComRetryAsync(url, ct);
            if (!resp.IsSuccessStatusCode) return new List<NearPlaceResult>();

            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("items", out var items) ||
                items.ValueKind != JsonValueKind.Array)
                return new List<NearPlaceResult>();

            var lista = new List<NearPlaceResult>();
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

                lista.Add(new NearPlaceResult
                {
                    Name = string.IsNullOrWhiteSpace(name) ? "Unidade de Saúde" : name.Trim(),
                    Address = address,
                    Latitude = plat,
                    Longitude = plng,
                    DistanceMeters = distance,
                    DistanceText = distance is null ? null : FormatDistance(distance.Value),
                    GoogleMapsLink = $"https://www.google.com/maps/search/?api=1&query={plat.ToString(CultureInfo.InvariantCulture)},{plng.ToString(CultureInfo.InvariantCulture)}"
                });
            }

            return lista;
        }

        // ---------- Helpers ----------
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

        private static string FormatDistance(int meters) =>
            meters < 1000 ? $"{meters} m" : $"{(meters / 1000.0):0.0} km";

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
                    if ((int)resp.StatusCode == 429 || (int)resp.StatusCode >= 500)
                    {
                        if (i == maxTentativas) return resp;
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

    public sealed class NearPlaceResult
    {
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int? DistanceMeters { get; set; }
        public string? DistanceText { get; set; }
        public string GoogleMapsLink { get; set; } = "";
    }
}
