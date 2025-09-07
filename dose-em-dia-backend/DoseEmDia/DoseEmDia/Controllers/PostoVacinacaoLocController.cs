using System.Threading;
using System.Threading.Tasks;
using DoseEmDia.Services.Geo;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/localizacao")]
public class LocalizacaoController : ControllerBase
{
    private readonly PostoVacinacaoLocService _svc;

    public LocalizacaoController(PostoVacinacaoLocService svc) => _svc = svc;

    // GET /api/localizacao/proximos?endereco=Rua%20XV%20de%20Novembro,%20Blumenau,%20SC&raioMetros=8000&limite=3
    [HttpGet("proximos")]
    public async Task<IActionResult> Proximos([FromQuery] string endereco, [FromQuery] int raioMetros = 10_000, [FromQuery] int limite = 3, CancellationToken ct = default)
    {
        var near = await _svc.BuscarPostosMaisProximosAsync(endereco, raioMetros, limite, ct);
        var dto = PostoAdapter.ToPostoVacinacaoResponse(near);
        return Ok(dto);
    }
}
