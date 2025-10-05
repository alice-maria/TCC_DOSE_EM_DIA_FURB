using System.Net;
using System.Security.Cryptography;
using System.Text;
using DoseEmDia.Models.db;
using DoseEmDia.Models.Enums;
using DoseEmDia.Models;
using DoseEmDia.Models.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace DoseEmDia.Helpers
{
    public sealed class EmailSettings
    {
        public string FromEmail { get; set; } = "nao-responder@doseemdia.com.br";
        public string FromName { get; set; } = "Dose em Dia";
        public bool ClickTrackingEnabled { get; set; } = true;
        public bool OpenTrackingEnabled { get; set; } = true;
    }

    public class EnvioEmail
    {
        private readonly ISendGridClient _sendGrid;
        private readonly EmailSettings _cfg;
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public EnvioEmail(ISendGridClient sendGrid, IOptions<EmailSettings> cfg, ApplicationDbContext db, IWebHostEnvironment env)
        {
            _sendGrid = sendGrid ?? throw new ArgumentNullException(nameof(sendGrid));
            _cfg = cfg?.Value ?? throw new ArgumentNullException(nameof(cfg));
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        public async Task EnviarEmailAsync(string destinatario, string assunto, string corpoHtml, CancellationToken ct = default)
        {
            var msg = MailHelper.CreateSingleEmail(
                from: new EmailAddress(_cfg.FromEmail, _cfg.FromName),
                to: new EmailAddress(destinatario),
                subject: assunto,
                plainTextContent: StripHtml(corpoHtml),
                htmlContent: corpoHtml
            );

            ConfigureTracking(msg);
            var resp = await _sendGrid.SendEmailAsync(msg, ct);
            await ThrowIfFailedAsync(resp, $"Falha ao enviar o e-mail para {destinatario}");
        }

        public async Task EnviarEmailRedefinicaoSenhaAsync(string emailDestino, string token, CancellationToken ct = default)
        {
            var tokenEncoded = WebUtility.UrlEncode(token);

            var corpoHtml = $@"
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
            </html>";

            await EnviarEmailAsync(emailDestino, "Redefinição de Senha", corpoHtml, ct);
        }

        public async Task DispararCampanhaAsync(string caminhoImagem, CancellationToken ct = default)
        {
            if (!File.Exists(caminhoImagem))
            {
                var arquivo = Path.GetFileName(caminhoImagem); 
                var tentativa1 = Path.Combine(_env.WebRootPath ?? string.Empty, "email", "banners", arquivo);

                var tentativa2 = Path.Combine(_env.ContentRootPath ?? string.Empty, "wwwroot", "email", "banners", arquivo);

                if (File.Exists(tentativa1))
                {
                    caminhoImagem = tentativa1;
                }
                else if (File.Exists(tentativa2))
                {
                    caminhoImagem = tentativa2;
                }
                else
                {
                    throw new FileNotFoundException("Imagem da campanha não encontrada.", tentativa1);
                }
            }

            var bytes = await File.ReadAllBytesAsync(caminhoImagem, ct);

            const string contentId = "bannerCampanha";

            const string assunto = "Campanha de Vacinação";
            const string html = @"
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

            var destinatarios = await _db.Usuario
                .AsNoTracking()
                .Where(u => !string.IsNullOrEmpty(u.Email) && u.ReceberNotificacoes == true)
                .Select(u => new { u.IdUser, u.Email })
                .ToListAsync();

            var notificacoes = new List<Notificacao>(destinatarios.Count);

            foreach (var u in destinatarios)
            {
                bool enviado = false;

                try
                {
                    var msg = new SendGridMessage
                    {
                        From = new EmailAddress(_cfg.FromEmail, "Dose em Dia – Campanha de Vacinação"),
                        Subject = assunto,
                        HtmlContent = html,
                        PlainTextContent = StripHtml(html)
                    };
                    msg.AddTo(new EmailAddress(u.Email));
                    ConfigureTracking(msg);

                    msg.AddAttachment(new Attachment
                    {
                        Content = Convert.ToBase64String(bytes),
                        Type = GetMimeType(caminhoImagem), // ex: "image/jpeg"
                        Filename = Path.GetFileName(caminhoImagem),
                        Disposition = "inline",
                        ContentId = contentId
                    });

                    var resp = await _sendGrid.SendEmailAsync(msg, ct);
                    await ThrowIfFailedAsync(resp, $"Falha ao enviar campanha para {u.Email}");
                    enviado = true;
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
                    EmailEnviado = enviado,
                    Visualizada = false
                });
            }

            if (notificacoes.Count > 0)
            {
                _db.Notificacao.AddRange(notificacoes);
                await _db.SaveChangesAsync(ct);
            }
        }

        public string GerarToken()
        {
            Span<byte> tokenData = stackalloc byte[32];
            RandomNumberGenerator.Fill(tokenData);
            return Convert.ToBase64String(tokenData);
        }

        public async Task EnviarEmailSuporteAsync(string nomeUsuario, string emailUsuario, string mensagem, string assunto = "ERROR / MELHORIA", string destinoEquipe = "notificadoseemdia@gmail.com", CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(emailUsuario))
                throw new ArgumentException("O e-mail do usuário é obrigatório.", nameof(emailUsuario));

            if (string.IsNullOrWhiteSpace(mensagem))
                throw new ArgumentException("A mensagem não pode estar vazia.", nameof(mensagem));

            var tz = GetTimeZone();
            var agoraSp = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz);
            var recebidoEm = $"{agoraSp:dd/MM/yyyy HH:mm} (Horário de Brasília)";
            var protocolo = Convert.ToHexString(RandomNumberGenerator.GetBytes(6));

            var body = $@"
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
            var msg = new SendGridMessage
            {
                From = new EmailAddress(_cfg.FromEmail, "Dose em Dia – Formulário"),
                Subject = $"[Dose em Dia | Suporte] {assunto} - {protocolo}",
                HtmlContent = body,
                PlainTextContent = StripHtml(body)
            };

            msg.AddTo(new EmailAddress(destinoEquipe));
            ConfigureTracking(msg);

            // Reply-To para o usuário
            msg.ReplyTo = new EmailAddress(
                emailUsuario,
                string.IsNullOrWhiteSpace(nomeUsuario) ? emailUsuario : nomeUsuario
            );

            var resp = await _sendGrid.SendEmailAsync(msg, ct);
            await ThrowIfFailedAsync(resp, "Falha ao enviar o e-mail de suporte.");
        }

        private static string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;
            var sb = new StringBuilder(html.Length);
            bool inside = false;
            foreach (var ch in html)
            {
                if (ch == '<') { inside = true; continue; }
                if (ch == '>') { inside = false; continue; }
                if (!inside) sb.Append(ch);
            }
            return WebUtility.HtmlDecode(sb.ToString());
        }

        private static string GetMimeType(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }

        private static TimeZoneInfo GetTimeZone()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo"); }
            catch
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"); }
                catch { return TimeZoneInfo.Local; }
            }
        }

        private void ConfigureTracking(SendGridMessage msg)
        {
            msg.SetClickTracking(_cfg.ClickTrackingEnabled, _cfg.ClickTrackingEnabled);
            msg.SetOpenTracking(_cfg.OpenTrackingEnabled);
        }

        private static async Task ThrowIfFailedAsync(Response resp, string contextMessage)
        {
            if (resp.IsSuccessStatusCode) return;

            string body = await resp.Body.ReadAsStringAsync();
            throw new EmailException($"{contextMessage}. Status: {(int)resp.StatusCode}. Detalhes: {body}");
        }
    }
}
