using DoseEmDia.Helpers;
using DoseEmDia.Models.Enums;
using DoseEmDia.Models;

namespace DoseEmDia.Services
{
    public class CampanhaVacinacaoService
    {
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
}
