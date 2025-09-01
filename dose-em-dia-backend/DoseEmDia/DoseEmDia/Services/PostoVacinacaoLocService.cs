using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DoseEmDia.Controllers.DTO;
using DoseEmDia.Controllers.Helpers;
using DoseEmDia.Models;
using DoseEmDia.Models.db;
using DoseEmDia.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DoseEmDia.Services
{
    public class PostoVacinacaoLocService : IPostoVacinacaoService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private const int LIMITE_REQUISICOES = 20;

        public PostoVacinacaoLocService(
            ApplicationDbContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(8);
            _configuration = configuration;
        }

        public async Task<IReadOnlyList<PostoVacinacaoResponse>> BuscarPostosVacinaAsync(
            int usuarioId,
            CancellationToken ct = default)
        {
            // --- rate limit simples (não propagar ct de request no SaveChanges) ---
            for (var tentativa = 0; tentativa < 3; tentativa++)
            {
                var contador = await _context.ContadorRequisicoes.FirstOrDefaultAsync(ct);
                if (contador is null)
                {
                    contador = new ContadorRequisicoes { Id = 1, Requisicoes = 0 };
                    _context.ContadorRequisicoes.Add(contador);
                    await _context.SaveChangesAsync(CancellationToken.None);
                }

                if (contador.Requisicoes >= LIMITE_REQUISICOES)
                    throw new InvalidOperationException("Limite de requisições atingido. Entre em contato com o suporte.");

                contador.Requisicoes++;

                try
                {
                    await _context.SaveChangesAsync(CancellationToken.None);
                    break;
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (tentativa == 2) throw;
                    await Task.Delay(50);
                }
            }

            // --- usuário + endereço ---
            using var dbCts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var usuario = await _context.Usuario
                .Include(u => u.Endereco)
                .FirstOrDefaultAsync(u => u.IdUser == usuarioId, dbCts.Token);

            if (usuario?.Endereco == null)
                throw new InvalidOperationException("Endereço do usuário não encontrado.");

            var cidadeAlvo = (usuario.Endereco.Cidade ?? "").Trim();
            var ufAlvo = (usuario.Endereco.Estado ?? "").Trim();

            // --- geocode com candidatos ---
            (double lat, double lng)? coord = null;
            foreach (var termo in CandidatosEndereco(usuario.Endereco))
            {
                coord = await GeocodeHereAsync(termo, ct);
                if (coord is not null) break;
            }

            if (coord is null)
                throw new InvalidOperationException("Não foi possível obter coordenadas para o endereço informado.");

            var (latitude, longitude) = coord.Value;

            // --- browse com termos e fallback por categorias + raio progressivo ---
            var raios = new[] { 5000, 10000, 15000 };
            var termos = new[] { "Unidade Básica de Saúde", "UBS", "Posto de Saúde", "Unidade de Saúde", "Clínica" };
            var agregados = new List<(int Dist, PostoVacinacaoResponse Resp)>();

            foreach (var raio in raios)
            {
                foreach (var termo in termos)
                {
                    var blocos = await BrowseHereTextoAsync(latitude, longitude, raio, termo, cidadeAlvo, ufAlvo, ct);
                    agregados.AddRange(blocos);
                    if (agregados.Count >= 3) return Top3(agregados);
                }

                var cats = await BrowseHereCategoriasAsync(latitude, longitude, raio, cidadeAlvo, ufAlvo, ct);
                agregados.AddRange(cats);
                if (agregados.Count >= 3) return Top3(agregados);
            }

            return Top3(agregados);
        }

        // -------------------- HERE: Geocode --------------------
        private async Task<(double lat, double lng)?> GeocodeHereAsync(string enderecoCompleto, CancellationToken ct)
        {
            var apiKey = _configuration["Here:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Chave da HERE API não configurada (Here:ApiKey).");

            var url =
                "https://geocode.search.hereapi.com/v1/geocode" +
                $"?q={Uri.EscapeDataString(enderecoCompleto)}" +
                $"&in=countryCode:BRA&lang=pt-BR&limit=3" +
                $"&apiKey={apiKey}";

            using var resp = await GetComRetryAsync(url, ct);
            if (!resp.IsSuccessStatusCode) return null;

            var json = await resp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("items", out var items) ||
                items.ValueKind != JsonValueKind.Array ||
                items.GetArrayLength() == 0)
                return null;

            JsonElement? melhor = null;
            int melhorScore = -1;
            foreach (var item in items.EnumerateArray())
            {
                int score = 0;
                if (item.TryGetProperty("resultType", out var rt))
                {
                    switch (rt.GetString())
                    {
                        case "houseNumber": score = 3; break;
                        case "street": score = 2; break;
                        case "locality": score = 1; break;
                    }
                }
                if (score > melhorScore) { melhor = item; melhorScore = score; }
            }

            var escolhido = melhor.HasValue ? melhor.Value : items.EnumerateArray().First();
            if (!escolhido.TryGetProperty("position", out var pos)) return null;

            if (pos.TryGetProperty("lat", out var latEl) &&
                pos.TryGetProperty("lng", out var lngEl) &&
                latEl.TryGetDouble(out var lat) &&
                lngEl.TryGetDouble(out var lng))
                return (lat, lng);

            return null;
        }

        // -------------------- HERE: Browse (texto) --------------------
        private async Task<List<(int Dist, PostoVacinacaoResponse Resp)>> BrowseHereTextoAsync(
            double latitude, double longitude, int raio, string termo,
            string cidadeAlvo, string ufAlvo, CancellationToken ct)
        {
            var apiKey = _configuration["Here:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                return new List<(int, PostoVacinacaoResponse)>();

            var baseCoord = $"{latitude.ToString(CultureInfo.InvariantCulture)},{longitude.ToString(CultureInfo.InvariantCulture)}";
            var q = Uri.EscapeDataString(termo);

            var url =
                "https://discover.search.hereapi.com/v1/browse" +
                $"?q={q}" +
                $"&in=circle:{baseCoord};r={raio}" +
                $"&at={baseCoord}" +
                $"&limit=20" +
                $"&lang=pt-BR" +
                $"&apiKey={apiKey}";

            using var resp = await GetComRetryAsync(url, ct);
            if (!resp.IsSuccessStatusCode) return new List<(int, PostoVacinacaoResponse)>();

            var json = await resp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Array)
                return new List<(int, PostoVacinacaoResponse)>();

            return MapearRespostas(items, cidadeAlvo, ufAlvo);
        }

        // -------------------- HERE: Browse (categorias) --------------------
        private async Task<List<(int Dist, PostoVacinacaoResponse Resp)>> BrowseHereCategoriasAsync(
            double latitude, double longitude, int raio,
            string cidadeAlvo, string ufAlvo, CancellationToken ct)
        {
            var apiKey = _configuration["Here:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                return new List<(int, PostoVacinacaoResponse)>();

            // UBS geralmente aparece como clínica municipal; hospital como fallback.
            var categories = "health-care.clinic,health-care.hospital";
            var baseCoord = $"{latitude.ToString(CultureInfo.InvariantCulture)},{longitude.ToString(CultureInfo.InvariantCulture)}";

            var url =
                "https://discover.search.hereapi.com/v1/browse" +
                $"?categories={categories}" +
                $"&in=circle:{baseCoord};r={raio}" +
                $"&at={baseCoord}" +
                $"&limit=20" +
                $"&lang=pt-BR" +
                $"&apiKey={apiKey}";

            using var resp = await GetComRetryAsync(url, ct);
            if (!resp.IsSuccessStatusCode) return new List<(int, PostoVacinacaoResponse)>();

            var json = await resp.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Array)
                return new List<(int, PostoVacinacaoResponse)>();

            return MapearRespostas(items, cidadeAlvo, ufAlvo);
        }

        // -------------------- Mapeamento comum + filtro tolerante --------------------
        private static List<(int Dist, PostoVacinacaoResponse Resp)> MapearRespostas(
            JsonElement items, string cidadeAlvo, string ufAlvo)
        {
            static string Norm(string? s)
            {
                if (string.IsNullOrWhiteSpace(s)) return "";
                var nf = s.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
                var sb = new StringBuilder(nf.Length);
                foreach (var ch in nf)
                {
                    var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
                    if (cat != UnicodeCategory.NonSpacingMark) sb.Append(ch);
                }
                return sb.ToString().Normalize(NormalizationForm.FormC);
            }

            var alvoCidade = Norm(cidadeAlvo);
            var alvoUF = Norm(ufAlvo);

            bool UFMatch(string v)
            {
                var n = Norm(v);
                if (string.IsNullOrEmpty(alvoUF)) return true;
                if (n == alvoUF) return true;
                if (alvoUF == "sc" && (n == "sc" || n == "santacatarina")) return true;
                return false;
            }

            var lista = new List<(int Dist, PostoVacinacaoResponse Resp)>();

            foreach (var item in items.EnumerateArray())
            {
                if (!item.TryGetProperty("address", out var address)) continue;

                string GetAddr(string prop) =>
                    address.TryGetProperty(prop, out var el) ? (el.GetString() ?? string.Empty) : string.Empty;

                var logradouro = GetAddr("street");
                var bairro = GetAddr("district");
                var cidade = GetAddr("city");
                var estado = GetAddr("state");

                // filtra apenas se info existir em ambos os lados
                if (!string.IsNullOrWhiteSpace(cidadeAlvo) && !string.IsNullOrWhiteSpace(cidade))
                    if (Norm(cidade) != alvoCidade) continue;

                if (!string.IsNullOrWhiteSpace(ufAlvo) && !string.IsNullOrWhiteSpace(estado))
                    if (!UFMatch(estado)) continue;

                var distanciaMetros = item.TryGetProperty("distance", out var distEl)
                    ? distEl.GetInt32() : int.MaxValue;

                if (!item.TryGetProperty("position", out var pos)) continue;
                if (!pos.TryGetProperty("lat", out var latEl)) continue;
                if (!pos.TryGetProperty("lng", out var lngEl)) continue;

                var lat = latEl.GetDouble();
                var lng = lngEl.GetDouble();

                var nome = item.TryGetProperty("title", out var titleEl) ? (titleEl.GetString() ?? "") : "";

                var resp = new PostoVacinacaoResponse
                {
                    Nome = string.IsNullOrWhiteSpace(nome) ? "Unidade de Saúde" : nome,
                    EnderecoCompleto = $"{logradouro}, {bairro} - {cidade}/{estado}"
                        .Trim()
                        .TrimStart(',')
                        .Replace(" ,", ","),
                    Distancia = FormatacaoHelper.FormatarDistancia(distanciaMetros),
                    LinkGoogleMaps = GerarLinkGoogleMaps(lat, lng)
                };

                lista.Add((distanciaMetros, resp));
            }

            return lista
                .GroupBy(x => $"{x.Resp.Nome}|{x.Resp.EnderecoCompleto}|{x.Resp.LinkGoogleMaps}")
                .Select(g => g.OrderBy(t => t.Dist).First())
                .OrderBy(t => t.Dist)
                .ToList();
        }

        // -------------------- Montagem de endereço (candidatos) --------------------
        private static IEnumerable<string> CandidatosEndereco(Endereco e)
        {
            string J(string? s) => string.IsNullOrWhiteSpace(s) ? "" : s.Trim();
            var log = J(e.Logradouro);
            var bai = J(e.Bairro);
            var cid = J(e.Cidade);
            var uf = J(e.Estado);
            var cep = new string((e.CEP ?? "").Where(char.IsDigit).ToArray());

            var full = string.Join(", ", new[] { log, bai, cid, uf }.Where(s => !string.IsNullOrWhiteSpace(s)).Append("Brasil"));
            if (!string.IsNullOrWhiteSpace(full)) yield return full;                    // Rua, Bairro, Cidade, UF, Brasil
            if (!string.IsNullOrWhiteSpace(cep) && cep.Length == 8) yield return $"{cep}, Brasil"; // CEP, Brasil
            if (!string.IsNullOrWhiteSpace(cid) && !string.IsNullOrWhiteSpace(uf)) yield return $"{cid}, {uf}, Brasil"; // Cidade, UF, Brasil
        }

        // -------------------- HTTP com retry leve --------------------
        private async Task<HttpResponseMessage> GetComRetryAsync(string url, CancellationToken ct)
        {
            const int maxTentativas = 3;
            for (int i = 1; i <= maxTentativas; i++)
            {
                try
                {
                    var resp = await _httpClient.GetAsync(url, ct);
                    if ((int)resp.StatusCode == 429 || (int)resp.StatusCode >= 500)
                    {
                        if (i == maxTentativas) return resp;
                        await Task.Delay(200 * i);
                        continue;
                    }
                    return resp;
                }
                catch (TaskCanceledException) when (!ct.IsCancellationRequested)
                {
                    if (i == maxTentativas) throw;
                    await Task.Delay(200 * i);
                }
                catch (HttpRequestException)
                {
                    if (i == maxTentativas) throw;
                    await Task.Delay(200 * i);
                }
            }
            throw new InvalidOperationException("Falha inesperada no retry HTTP.");
        }

        private static List<PostoVacinacaoResponse> Top3(List<(int Dist, PostoVacinacaoResponse Resp)> agregados) =>
            agregados
                .OrderBy(t => t.Dist)
                .Take(3)
                .Select(t => t.Resp)
                .ToList();

        private static string GerarLinkGoogleMaps(double latitude, double longitude) =>
            $"https://www.google.com/maps/search/?api=1&query={latitude.ToString(CultureInfo.InvariantCulture)},{longitude.ToString(CultureInfo.InvariantCulture)}";
    }
}
