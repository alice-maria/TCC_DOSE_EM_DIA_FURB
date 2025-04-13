using System.Net.Mail;
using System.Net;

namespace DoseEmDia.Models
{
    public class Notificacao
    {
        public string Titulo { get; set; }
        public string Mensagem { get; set; }
        public DateTime DataEnvio { get; set; }
        public string Email { get; set; }

        public Notificacao(string titulo, string mensagem, DateTime dataEnvio, string email)
        {
            Titulo = titulo;
            Mensagem = mensagem;
            DataEnvio = dataEnvio;
            Email = email;
        }

        public Notificacao() { }

        public void Enviar()
        {
            try
            {
                var (smtpServidor, porta) = ObterServidorSmtp(Email);

                string remetente = "notificadoseemdia@gmail.com";
                string senha = "Doseemdia2025";

                using (SmtpClient smtpClient = new SmtpClient(smtpServidor))
                {
                    smtpClient.Port = porta;
                    smtpClient.Credentials = new NetworkCredential(remetente, senha);
                    smtpClient.EnableSsl = true;

                    MailMessage mailMessage = new MailMessage
                    {
                        From = new MailAddress(remetente),
                        Subject = Titulo,
                        Body = Mensagem,
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(Email);

                    smtpClient.Send(mailMessage);
                    Console.WriteLine("E-mail enviado com sucesso!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
            }
        }
        public static (string servidor, int porta) ObterServidorSmtp(string email)
        {
            string dominio = email.Split('@')[1].ToLower();

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
