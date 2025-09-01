using DoseEmDia.Helpers;
using DoseEmDia.Models.Exceptions;
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

        [HttpPost("/api/suporte/mensagem")] 
        public async Task<IActionResult> EnviarMensagemSuporte([FromBody] SuporteInput dto, [FromServices] EnvioEmail envioEmail) 
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Mensagem))
                return BadRequest("E-mail e mensagem são obrigatórios.");

            try
            {
                await envioEmail.EnviarEmailSuporteAsync(
                    nomeUsuario: dto.Nome ?? "",
                    emailUsuario: dto.Email,
                    mensagem: dto.Mensagem,
                    assunto: string.IsNullOrWhiteSpace(dto.Assunto) ? "Contato via sistema" : dto.Assunto,
                    destinoEquipe: "notificadoseemdia@gmail.com"  
                );

                return Ok(new { ok = true });
            }
            catch (EmailException ex)   
            {
                return StatusCode(502, ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Erro interno ao enviar a mensagem de suporte.");
            }
        }

    }
    public class SuporteInput
    {
        public string? Nome { get; set; }
        public string Email { get; set; } = "";    
        public string? Assunto { get; set; }
        public string Mensagem { get; set; } = "";
    }

}
