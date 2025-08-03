using DoseEmDia.Helpers;
using DoseEmDia.Models;
using DoseEmDia.Models.db;
using DoseEmDia.Models.Enums;
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

        public async Task AtualizarPreferenciasNotificacao(int id, bool receberNotificacoes)
        {
            var usuario = await _context.Usuario.FindAsync(id);
            if (usuario == null)
                throw new Exception("Usuário não encontrado.");

            usuario.ReceberNotificacoes = receberNotificacoes;
            await _context.SaveChangesAsync();
        }

        public async Task EnviarCampanhaSePermitidoAsync(int usuarioId, string titulo, string mensagemHtml, byte[] imagemBytes)
        {
            var usuario = await _context.Usuario.FindAsync(usuarioId);
            if (usuario == null)
                throw new Exception("Usuário não encontrado.");

            // Verifica se o usuário optou por não receber campanhas
            if (!usuario.ReceberNotificacoes)
                return;

            // Chama o helper de envio de e-mail
            var emailHelper = new EnvioEmail();
            await emailHelper.EnviarEmailCampanhaAsync(usuario.Email, titulo, mensagemHtml, imagemBytes);

            // Registra no banco (se desejar manter histórico)
            var notificacao = new Notificacao
            {
                UsuarioId = usuarioId,
                Tipo = TipoNotificacao.CampanhaImunizacao,
                Mensagem = titulo, // ou um resumo da campanha
                DataEnvio = DateTime.Now
            };

            _context.Notificacao.Add(notificacao);
            await _context.SaveChangesAsync();
        }
    }

    public class PreferenciaNotificacaoRequest
    {
        public bool ReceberNotificacoes { get; set; }
    }

}
