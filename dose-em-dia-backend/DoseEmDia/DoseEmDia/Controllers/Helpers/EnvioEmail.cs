using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using DoseEmDia.Models.Exceptions;
using System.Net.Mime;

namespace DoseEmDia.Helpers
{
    public class EnvioEmail
    {
        private readonly string _remetente = "notificadoseemdia@gmail.com";
        private readonly string _senha = "cwtpslgbcnpxdkvu";

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
                Body = $"Clique no link para redefinir sua senha: http://localhost:8080/redefinir-senha?token={tokenEncoded}",
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

        public async Task EnviarEmailCampanhaAsync(string destinatario, string titulo, string mensagemHtml, byte[] imagemBytes)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_remetente),
                Subject = titulo,
                IsBodyHtml = true
            };
            mailMessage.To.Add(destinatario);

            var view = AlternateView.CreateAlternateViewFromString(mensagemHtml, null, MediaTypeNames.Text.Html);

            var imagem = new LinkedResource(new MemoryStream(imagemBytes), MediaTypeNames.Image.Jpeg)
            {
                ContentId = "bannerCampanha"
            };
            view.LinkedResources.Add(imagem);

            mailMessage.AlternateViews.Add(view);

            var (servidor, porta) = ObterServidorSmtp(destinatario); // pega o smtp e a porta conforme o e-mail destino

            using var smtpClient = new SmtpClient(servidor, porta)
            {
                Credentials = new NetworkCredential(_remetente, _senha),
                EnableSsl = true
            };

            await smtpClient.SendMailAsync(mailMessage);
        }

        public string GerarToken()
        {
            using var rng = new RNGCryptoServiceProvider();
            byte[] tokenData = new byte[32];
            rng.GetBytes(tokenData);
            return Convert.ToBase64String(tokenData);
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
