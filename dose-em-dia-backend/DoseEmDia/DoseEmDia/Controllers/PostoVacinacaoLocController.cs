using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoseEmDia.Services.Geo;
using DoseEmDia.Models.db; 

namespace DoseEmDia.Api.Controllers
{
    [ApiController]
    [Route("api/localizacao")]
    public class LocalizacaoController : ControllerBase
    {
        private readonly PostoVacinacaoLocService _svc;
        private readonly ApplicationDbContext _db;

        public LocalizacaoController(PostoVacinacaoLocService svc, ApplicationDbContext db)
        {
            _svc = svc ?? throw new ArgumentNullException(nameof(svc));
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        [HttpGet("proximos")]
        public async Task<IActionResult> Proximos([FromQuery] string? endereco, [FromQuery] int? usuarioId, [FromQuery] int raioMetros = 10_000, [FromQuery] int limite = 3, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(endereco))
            {
                if (!usuarioId.HasValue)
                    return BadRequest("Informe 'endereco' ou 'usuarioId'.");

                var usuario = await _db.Usuario
                    .Include(x => x.Endereco)
                    .FirstOrDefaultAsync(x => x.IdUser == usuarioId.Value, ct);

                if (usuario is null)
                    return NotFound("Usuário não encontrado.");

                if (usuario.Endereco is null)
                    return BadRequest("Usuário não possui endereço cadastrado.");

                var e = usuario.Endereco;

                string cep = Regex.Replace(e.CEP ?? string.Empty, "[^0-9]", "");
                string rua = (e.Logradouro ?? string.Empty).Trim();
                string bairro = (e.Bairro ?? string.Empty).Trim();
                string cidade = (e.Cidade ?? string.Empty).Trim();
                string uf = (e.Estado ?? string.Empty).Trim();

                endereco =
                    $"{rua}{(string.IsNullOrWhiteSpace(bairro) ? "" : $" - {bairro}")}, {cidade}/{uf}" +
                    $"{(string.IsNullOrWhiteSpace(cep) ? "" : $", {cep}")}";
            }

            var near = await _svc.BuscarPostosMaisProximosAsync(endereco!, raioMetros, limite, ct);

            var dto = PostoAdapter.ToPostoVacinacaoResponse(near);
            return Ok(dto);
        }
    }
}
