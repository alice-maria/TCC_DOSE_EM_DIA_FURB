namespace DoseEmDia.Models.Exceptions
{
    public class EmailException : Exception
    {
        public EmailException(string mensagem, Exception exceptions)
           : base(mensagem, exceptions) { }
    }
}
