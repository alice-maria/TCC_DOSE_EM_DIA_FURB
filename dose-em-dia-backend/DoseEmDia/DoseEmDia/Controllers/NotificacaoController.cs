using Microsoft.AspNetCore.Mvc;

namespace DoseEmDia.Controllers
{
    [ApiController]
    [Route("api/notificacoes")]
    public class NotificacaoController : ControllerBase
    {
        private readonly NotificacaoService _service;

        public NotificacaoController(NotificacaoService service)
        {
            _service = service;
        }

        [HttpGet("usuario/{cpf}/historico")]
        public async Task<IActionResult> ListarHistoricoPorCpf(string cpf)
        {
            try
            {
                var notificacoes = await _service.ListarHistoricoPorCpf(cpf);
                return Ok(notificacoes);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
