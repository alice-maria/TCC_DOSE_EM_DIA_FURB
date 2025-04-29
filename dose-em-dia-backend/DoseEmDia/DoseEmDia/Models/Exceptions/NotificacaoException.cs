namespace DoseEmDia.Models.Exceptions
{
    public class NotificacaoException : Exception
    {
        public NotificacaoException(string mensagem) : base(mensagem) { }

        public static NotificacaoException ErroEnvioEmail(string mensagem) =>
            new NotificacaoException($"Erro ao enviar e-mail: {mensagem}");

        public static NotificacaoException ErroEnvioSMS(string mensagem) =>
            new NotificacaoException($"Erro ao enviar SMS: {mensagem}");
    }
}
