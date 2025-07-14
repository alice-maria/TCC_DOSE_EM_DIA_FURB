using Microsoft.AspNetCore.Mvc;

namespace DoseEmDia.Controllers
{
    [ApiController]
    [Route("api/comprovante")]
    public class ComprovanteController : ControllerBase
    {
        private readonly ComprovanteService _comprovanteService;

        public ComprovanteController(ComprovanteService comprovanteService)
        {
            _comprovanteService = comprovanteService;
        }

        [HttpGet("{usuarioId}/gerarComprovante")]
        public async Task<IActionResult> GerarPdfComprovante(int usuarioId)
        {
            try
            {
                var (conteudoPdf, nomeArquivo) = await _comprovanteService.GerarPdfComprovante(usuarioId);
                return File(conteudoPdf, "application/pdf", nomeArquivo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro ao gerar comprovante: " + ex.Message);
            }
        }

    }
}
