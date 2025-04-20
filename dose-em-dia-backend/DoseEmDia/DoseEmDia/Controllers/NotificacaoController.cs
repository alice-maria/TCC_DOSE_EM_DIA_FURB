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
    }
}
