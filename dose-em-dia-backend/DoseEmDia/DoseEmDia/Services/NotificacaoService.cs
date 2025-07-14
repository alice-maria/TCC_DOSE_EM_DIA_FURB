using DoseEmDia.Models;
using DoseEmDia.Models.db;
using Microsoft.EntityFrameworkCore;

namespace DoseEmDia.Controllers
{
    public class NotificacaoService
    {
        private readonly ApplicationDbContext _context;

        public NotificacaoService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Notificacao>> ListarHistoricoPorCpf(string cpf)
        {
            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.CPF == cpf);

            if (usuario == null)
                throw new Exception("Usuário não encontrado.");

            return await _context.Notificacao
                .Where(n => n.UsuarioId == usuario.IdUser)
                .OrderByDescending(n => n.DataEnvio)
                .ToListAsync();
        }
    }
}
