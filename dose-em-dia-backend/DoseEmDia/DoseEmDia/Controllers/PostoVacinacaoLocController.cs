using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoseEmDia.Services.Geo;
using DoseEmDia.Models.db;

namespace DoseEmDia.Api.Controllers
{
    [ApiController]
    [Route("api/localizacao")]
    public class PostoVacinacaoLocController : ControllerBase
    {
        private readonly PostoVacinacaoLocService _servico;
        private readonly ApplicationDbContext _db;

        public PostoVacinacaoLocController(PostoVacinacaoLocService servico, ApplicationDbContext db)
        {
            _servico = servico ?? throw new ArgumentNullException(nameof(servico));
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        [HttpGet("proximos")]
        public async Task<IActionResult> ObterProximos([FromQuery] string? endereco, [FromQuery] int? usuarioId, [FromQuery] int raioMetros = 10_000, [FromQuery] int limite = 3, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(endereco))
            {
                if (!usuarioId.HasValue)
                    return BadRequest("Id não identificado.");

                var usuario = await _db.Usuario
                    .Include(u => u.Endereco)
                        .ThenInclude(e => e.Cep)
                            .ThenInclude(c => c.Cidade)
                                .ThenInclude(ci => ci.Estado)
                    .FirstOrDefaultAsync(u => u.IdUser == usuarioId.Value, ct);

                if (usuario is null)
                    return NotFound("Usuário não encontrado.");

                var e = usuario.Endereco;
                if (e is null)
                    return BadRequest("Usuário não possui endereço cadastrado.");

                string logradouro = e.Logradouro?.Trim() ?? string.Empty;
                string numero = e.Numero?.Trim() ?? string.Empty;

                string bairro = e.Cep?.Bairro?.Trim() ?? string.Empty;
                string cidade = e.Cep?.Cidade?.Nome?.Trim() ?? string.Empty;
                string uf = e.Cep?.Cidade?.Estado?.Uf?.Trim() ?? string.Empty; 
                string cep = e.Cep?.Codigo?.Trim() ?? string.Empty;

                if (!string.IsNullOrEmpty(cep))
                {
                    cep = Regex.Replace(cep, @"\D", "");
                    if (cep.Length == 8)
                        cep = $"{cep[..5]}-{cep[5..]}";
                }

                var partes = new List<string>();

                var primeiraLinha = string.Join(", ",
                    new[]
                    {
                        string.IsNullOrWhiteSpace(logradouro) ? null : logradouro,
                        string.IsNullOrWhiteSpace(numero)     ? null : numero
                    }.Where(x => x is not null));

                if (!string.IsNullOrWhiteSpace(primeiraLinha))
                {
                    if (!string.IsNullOrWhiteSpace(bairro))
                        primeiraLinha += $" - {bairro}";

                    partes.Add(primeiraLinha);
                }

                var cidadeUf = string.Join("/",
                    new[]
                    {
                        string.IsNullOrWhiteSpace(cidade) ? null : cidade,
                        string.IsNullOrWhiteSpace(uf)     ? null : uf
                    }.Where(x => x is not null));

                if (!string.IsNullOrWhiteSpace(cidadeUf))
                    partes.Add(cidadeUf);

                if (!string.IsNullOrWhiteSpace(cep))
                    partes.Add(cep);

                endereco = string.Join(", ", partes);

                if (string.IsNullOrWhiteSpace(endereco))
                {
                    if (string.IsNullOrWhiteSpace(cep))
                        return BadRequest("Não foi possível montar o endereço do usuário.");

                    endereco = cep;
                }
            }

            var proximos = await _servico.ObterPostosMaisProximos(endereco!, raioMetros, limite, ct);
            var resposta = PostoAdapter.RespostaPostoVacinacao(proximos);

            return Ok(resposta);
        }
    }
}