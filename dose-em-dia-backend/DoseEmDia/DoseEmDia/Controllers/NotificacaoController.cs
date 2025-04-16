using DoseEmDia.Models.db;
using DoseEmDia.Models;
using Microsoft.AspNetCore.Mvc;

namespace DoseEmDia.Controllers
{
    public class NotificacaoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotificacaoController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{usuarioId}/notificacoes")]
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
