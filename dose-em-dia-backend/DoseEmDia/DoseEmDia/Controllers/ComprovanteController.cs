using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DoseEmDia.Controllers
{
    [ApiController]
    [Route("api/comprovante")]
    public class ComprovanteController : ControllerBase
    {
        private readonly ComprovanteService _comprovanteService;
        private readonly IWebHostEnvironment _env;

        public ComprovanteController(ComprovanteService service, IWebHostEnvironment env)
        {
            _comprovanteService = service;
            _env = env;
        }

        // GET /api/comprovante/123/gerarComprovante?debug=1
        [HttpGet("{usuarioId}/gerarComprovante")]
        public async Task<IActionResult> GerarPdfComprovante(int usuarioId, [FromQuery] int? debug = null)
        {
            try
            {
                var (bytes, nomeArquivo) = await _comprovanteService.GerarPdfComprovante(usuarioId);
                var fileName = string.IsNullOrWhiteSpace(nomeArquivo) ? $"comprovante-{usuarioId}.pdf" : nomeArquivo;
                return File(bytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                // Em produção, mensagem curta. Em dev ou debug=1, detalha.
                var detalhar = (debug == 1) || _env.IsDevelopment();
                var corpo = detalhar
                    ? MontarDiagnostico(ex)
                    : "Falha ao gerar comprovante. Tente novamente mais tarde.";
                return StatusCode(500, corpo);
            }
        }

        private static string MontarDiagnostico(Exception ex)
        {
            var inner = ex.InnerException != null
                ? $"{ex.InnerException.GetType().Name}: {ex.InnerException.Message}"
                : "—";

            var stackTop = string.Join('\n', (ex.StackTrace ?? "").Split('\n').Take(10));
            return
$@"Erro ao gerar comprovante
Tipo: {ex.GetType().Name}
Mensagem: {ex.Message}
Inner: {inner}
Stack (topo):
{stackTop}";
        }
    }
}
