using DoseEmDia.Models.db;
using Microsoft.AspNetCore.Mvc;

namespace DoseEmDia.Controllers
{
    [ApiController]
    [Route("api/notificacoes")]
    public class NotificacaoController : ControllerBase
    {
        private readonly NotificacaoService _service;
        private readonly ApplicationDbContext _context;
        public NotificacaoController(NotificacaoService service, ApplicationDbContext context)
        {
            _service = service;
            _context = context;
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

        [HttpPut("usuario/{id}/ReceberNotificacoes")]
        public async Task<IActionResult> AtualizarPreferenciasNotificacao(int id, [FromBody] PreferenciaNotificacaoRequest request)
        {
            try
            {
                await _service.AtualizarPreferenciasNotificacao(id, request.ReceberNotificacoes);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
