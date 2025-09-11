namespace DoseEmDia.Models.Exceptions
{
    public class EmailException : Exception
    {
        public EmailException() { }

        public EmailException(string mensagem)
            : base(mensagem) { }

        public EmailException(string mensagem, Exception exceptions)
           : base(mensagem, exceptions) { }
    }
}
