using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using DoseEmDia.Models.Exceptions;

namespace DoseEmDia.Helpers
{
    public class EnvioEmail
    {
        private readonly string _remetente = "notificadoseemdia@gmail.com";
        private readonly string _senha = "cwtp slgb cnpx dkvu";

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

            var mail = new MailMessage
            {
                From = new MailAddress(_remetente),
                Subject = "Redefinição de Senha",
                Body = $"Clique no link para redefinir sua senha: http://localhost:8080/redefinir-senha?token={token}",
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
        public string GerarToken()
        {
            using var rng = new RNGCryptoServiceProvider();
            byte[] tokenData = new byte[32];
            rng.GetBytes(tokenData);
            return Convert.ToBase64String(tokenData);
        }

    }
}
