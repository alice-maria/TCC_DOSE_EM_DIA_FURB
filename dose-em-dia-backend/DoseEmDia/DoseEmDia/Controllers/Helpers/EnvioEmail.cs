using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using DoseEmDia.Models.Exceptions;
using System.Net.Mime;
using DoseEmDia.Models.db;
using Microsoft.EntityFrameworkCore;
using DoseEmDia.Models.Enums;
using DoseEmDia.Models;

namespace DoseEmDia.Helpers
{
    public class EnvioEmail
    {
        private readonly string _remetente = "notificadoseemdia@gmail.com";
        private readonly string _senha = "cwtpslgbcnpxdkvu";
        private readonly ApplicationDbContext _db;

        public EnvioEmail(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task EnviarEmailAsync(string destinatario, string assunto, string corpoHtml)
        {
            var (smtpServidor, porta) = ObterServidorSmtp(destinatario);

            using var smtpClient = new SmtpClient(smtpServidor, porta)
            {
                Credentials = new NetworkCredential(_remetente, _senha),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_remetente),
                Subject = assunto,
                Body = corpoHtml,
                IsBodyHtml = true
            };

            mailMessage.To.Add(destinatario);

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (SmtpException ex)
            {
                throw new EmailException("Falha ao enviar o e-mail. Verifique as configurações de SMTP ou conectividade.", ex);
            }
            catch (Exception ex)
            {
                throw new EmailException($"Erro inesperado ao enviar o e-mail para {destinatario}.", ex);
            }
        }

        public async Task EnviarEmailRedefinicaoSenhaAsync(string emailDestino, string token)
        {
            var (servidor, porta) = ObterServidorSmtp(emailDestino);

            using var smtp = new SmtpClient(servidor, porta)
            {
                Credentials = new NetworkCredential(_remetente, _senha),
                EnableSsl = true
            };

            var tokenEncoded = WebUtility.UrlEncode(token);

            var mail = new MailMessage
            {
                From = new MailAddress(_remetente),
                Subject = "Redefinição de Senha",
                Body = $@"
<html>
  <body style='font-family: Roboto, Arial, sans-serif; background-color: #FAFAFA; color: #202124; padding: 20px;'>
    <div style='max-width: 600px; margin: auto; background-color: #ffffff; border-radius: 8px; padding: 30px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);'>
      <h2 style='color: #f46c20;'>Redefinição de Senha</h2>
      <p>Olá,</p>
      <p>
        Recebemos uma solicitação para redefinir sua senha no sistema <strong>Dose em Dia</strong>,
        a plataforma de conscientização e acompanhamento de vacinas.
      </p>
      <p>
        Para prosseguir com a redefinição, clique no botão abaixo:
      </p>
      <p style='text-align: center; margin: 30px 0;'>
        <a href='https://dose-em-dia.up.railway.app/esqueci-redefinir-minha-senha?token={tokenEncoded}'
           style='background-color: #f46c20; color: white; padding: 14px 28px; text-decoration: none;
                  font-weight: 500; border-radius: 6px; display: inline-block;'>
          Redefinir senha
        </a>
      </p>
      <p>
        Caso não tenha solicitado esta redefinição, apenas ignore este e-mail. Nenhuma ação será realizada.
      </p>
      <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;' />
      <p style='font-size: 14px; color: #666;'>
        Atenciosamente,<br/>
        <strong>Equipe Dose em Dia</strong>
      </p>
    </div>
  </body>
</html>",

                IsBodyHtml = true
            };

            mail.To.Add(emailDestino);

            try
            {
                await smtp.SendMailAsync(mail);
            }
            catch (SmtpException ex)
            {
                throw new EmailException("Falha ao enviar o e-mail. Verifique as configurações de SMTP.", ex);
            }
            catch (Exception ex)
            {
                throw new EmailException("Erro ao tentar enviar o e-mail de redefinição de senha.", ex);
            }
        }

        public async Task DispararCampanhaAsync(string caminhoImagem)
        {
            if (!File.Exists(caminhoImagem))
                throw new FileNotFoundException("Imagem da campanha não encontrada.", caminhoImagem);

            byte[] imagemBytes = await File.ReadAllBytesAsync(caminhoImagem);

            const string mensagemHtml = @"
            <html>
              <body style=""font-family: Arial, sans-serif; line-height: 1.5; color: #333;"">
                <h2 style=""color:#2d89ef;"">Campanha de Vacinação</h2>
                <p>
                  Não perca a oportunidade de se proteger e proteger quem você ama. 
                  Participe da campanha de vacinação e mantenha sua saúde em dia.
                </p>
                <img src=""cid:bannerCampanha"" alt=""Campanha de Vacinação"" style=""max-width:100%; height:auto; margin: 15px 0;"" />
                <p>
                  Para saber mais detalhes sobre locais de vacinação, público-alvo e calendário, 
                  acesse o site oficial do Ministério da Saúde: <br/>
                  <a href=""https://www.gov.br/saude/pt-br/campanhas-da-saude/2025"" 
                     style=""color:#2d89ef; text-decoration:none; font-weight:bold;"">
                     Clique aqui para mais informações
                  </a>
                </p>
              </body>
            </html>";

            const string assunto = "Campanha de Vacinação";

            var destinatarios = await _db.Usuario
                .AsNoTracking()
                .Where(u => !string.IsNullOrEmpty(u.Email) && u.ReceberNotificacoes == true)
                .Select(u => new { u.IdUser, u.Email })
                .ToListAsync();

            var notificacoes = new List<Notificacao>(destinatarios.Count);

            foreach (var u in destinatarios)
            {
                bool enviadoComSucesso = false;

                try
                {
                    await EnviarEmailCampanha(u.Email, mensagemHtml, imagemBytes);
                    enviadoComSucesso = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Falha ao enviar e-mail para {u.Email}: {ex.Message}");
                }

                notificacoes.Add(new Notificacao
                {
                    UsuarioId = u.IdUser,
                    Tipo = TipoNotificacao.CampanhaImunizacao,
                    Titulo = assunto,
                    Mensagem = assunto,
                    DataEnvio = DateTime.UtcNow,
                    EmailEnviado = enviadoComSucesso,
                    Visualizada = false
                });
            }

            if (notificacoes.Count > 0)
            {
                _db.Notificacao.AddRange(notificacoes);
                await _db.SaveChangesAsync();
            }
        }

        private async Task EnviarEmailCampanha(string destinatario, string mensagemHtml, byte[] imagemBytes)
        {
            var (servidor, porta) = ObterServidorSmtp(_remetente);

            using var smtp = new SmtpClient(servidor, porta)
            {
                Credentials = new NetworkCredential(_remetente, _senha),
                EnableSsl = true
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(_remetente, "Dose em Dia – Campanha de Vacinação"),
                Subject = "Campanha de Vacinação",
                IsBodyHtml = true
            };

            mail.To.Add(destinatario);

            var view = AlternateView.CreateAlternateViewFromString(mensagemHtml, null, MediaTypeNames.Text.Html);

            var lr = new LinkedResource(new MemoryStream(imagemBytes), "image/jpeg")
            {
                ContentId = "bannerCampanha",
                TransferEncoding = TransferEncoding.Base64
            };

            view.LinkedResources.Add(lr);
            mail.AlternateViews.Add(view);

            await smtp.SendMailAsync(mail);
        }


        public string GerarToken()
        {
            using var rng = new RNGCryptoServiceProvider();
            byte[] tokenData = new byte[32];
            rng.GetBytes(tokenData);
            return Convert.ToBase64String(tokenData);
        }

        public async Task EnviarEmailSuporteAsync(string nomeUsuario, string emailUsuario, string mensagem, string assunto = "ERROR / MELHORIA", string destinoEquipe = "notificadoseemdia@gmail.com")
        {
            if (string.IsNullOrWhiteSpace(emailUsuario))
                throw new ArgumentException("O e-mail do usuário é obrigatório.", nameof(emailUsuario));

            if (string.IsNullOrWhiteSpace(mensagem))
                throw new ArgumentException("A mensagem não pode estar vazia.", nameof(mensagem));

            var (servidor, porta) = ObterServidorSmtp(_remetente);

            using var smtp = new SmtpClient(servidor, porta)
            {
                Credentials = new NetworkCredential(_remetente, _senha),
                EnableSsl = true
            };

            TimeZoneInfo tz;
            try
            {
                tz = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
            }
            catch
            {
                try
                {
                    tz = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
                }
                catch
                {
                    tz = TimeZoneInfo.Local;
                }
            }
            var agoraSp = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz);
            string recebidoEm = agoraSp.ToString("dd/MM/yyyy HH:mm") + " (Horário de Brasília)";

            string body = $@"
<!DOCTYPE html>
<html lang='pt-BR'>
  <body style='margin:0;padding:24px;background:#f6f8fb;
               font-family:-apple-system,BlinkMacSystemFont,Segoe UI,Roboto,Arial,sans-serif;color:#111;'>
    <table role='presentation' cellpadding='0' cellspacing='0' border='0' width='100%'>
      <tr>
        <td align='center'>
          <table role='presentation' cellpadding='0' cellspacing='0' border='0' width='100%'
                 style='max-width:680px;background:#ffffff;border:1px solid #e6e8eb;border-radius:12px;overflow:hidden;'>
            <tr>
              <td style='padding:16px 24px;background:#fff3e9;border-bottom:1px solid #ffe3cf'>
                <div style='display:flex;justify-content:space-between;align-items:center;gap:12px;'>
                  <h1 style='margin:0;font-size:18px;line-height:1.35;color:#d35400;'>Chamado de Suporte — Dose em Dia</h1>
                </div>
              </td>
            </tr>

            <tr>
              <td style='padding:20px 24px'>
                <table role='presentation' cellpadding='0' cellspacing='0' border='0' width='100%'
                       style='border-collapse:separate;border-spacing:0 8px;font-size:14px;'>
                  <tr>
                    <td width='160' style='color:#555;'>Nome</td>
                    <td style='color:#111;'>{WebUtility.HtmlEncode(nomeUsuario)}</td>
                  </tr>
                  <tr>
                    <td width='160' style='color:#555;'>E-mail</td>
                    <td style='color:#111;'>{WebUtility.HtmlEncode(emailUsuario)}</td>
                  </tr>
                  <tr>
                    <td width='160' style='color:#555;'>Recebido em</td>
                    <td style='color:#111;'>{WebUtility.HtmlEncode(recebidoEm)}</td>
                  </tr>
                  <tr>
                    <td width='160' style='color:#555;'>Mensagem:</td>
                    <td style='color:#111;'>{WebUtility.HtmlEncode(mensagem)}</td>
                  </tr>
                </table>

                <div style='height:16px'></div>
              </td>
            </tr>
          </table>
        </td>
      </tr>
    </table>
  </body>
</html>";
            var protocolo = $"{Convert.ToHexString(RandomNumberGenerator.GetBytes(6))}";

            using var mail = new MailMessage
            {
                From = new MailAddress(_remetente, "Dose em Dia – Formulário"),
                Subject = $"[Dose em Dia | Suporte] {assunto} - {protocolo}",
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(destinoEquipe);

            if (!string.IsNullOrWhiteSpace(emailUsuario))
                mail.ReplyToList.Add(new MailAddress(emailUsuario, string.IsNullOrWhiteSpace(nomeUsuario) ? emailUsuario : nomeUsuario));

            try
            {
                await smtp.SendMailAsync(mail);
            }
            catch (SmtpException ex)
            {
                throw new EmailException("Falha ao enviar o e-mail de suporte.", ex);
            }
            catch (Exception ex)
            {
                throw new EmailException("Erro inesperado ao enviar o e-mail de suporte.", ex);
            }
        }

        private static (string servidor, int porta) ObterServidorSmtp(string email)
        {
            var dominio = email.Split('@')[1].ToLower();

            return dominio switch
            {
                "gmail.com" => ("smtp.gmail.com", 587),
                "outlook.com" or "hotmail.com" => ("smtp.office365.com", 587),
                "yahoo.com" => ("smtp.mail.yahoo.com", 465),
                "icloud.com" => ("smtp.mail.me.com", 587),
                _ => ("smtp.sendgrid.net", 587)
            };
        }
    }
}
