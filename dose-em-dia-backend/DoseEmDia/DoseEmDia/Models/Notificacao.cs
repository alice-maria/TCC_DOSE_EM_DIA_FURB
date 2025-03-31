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
    }
}
