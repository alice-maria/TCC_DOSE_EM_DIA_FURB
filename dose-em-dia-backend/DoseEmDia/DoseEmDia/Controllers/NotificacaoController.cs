using DoseEmDia.Models.db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoseEmDia.Controllers
{
    [ApiController]
    [Route("api/notificacoes")]
    public class NotificacaoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificacaoController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{usuarioId}/listaNotificacoes")]
        public async Task<IActionResult> ListarNotificacoes(int usuarioId)
        {
            var notificacoes = await _context.Notificacao
                .Where(n => n.UsuarioId == usuarioId)
                .OrderByDescending(n => n.DataEnvio)
                .ToListAsync();

            return Ok(notificacoes);
        }
        
        [HttpGet("usuario/{cpf}/historico")] //endpoint novo para buscar notificações do usuário
        public async Task<IActionResult> ListarHistoricoPorCpf(string cpf)
        {
            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.CPF == cpf);

            if (usuario == null)
                return NotFound("Usuário não encontrado.");

            var notificacoes = await _context.Notificacao
                .Where(n => n.UsuarioId == usuario.IdUser)
                .OrderByDescending(n => n.DataEnvio)
                .ToListAsync();

            return Ok(notificacoes);
        }
    }
}
