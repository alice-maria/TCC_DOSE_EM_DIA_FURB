namespace DoseEmDia.Models.Exceptions
{
    public class LocalizacaoException : Exception
    {
        public LocalizacaoException(string mensagem) : base(mensagem) { }

        public static LocalizacaoException ErroConsultaHereAPI() =>
            new LocalizacaoException("Falha ao consultar a API da HERE Maps.");

        public static LocalizacaoException CoordenadaInvalida() =>
            new LocalizacaoException("Latitude ou Longitude inválida.");
    }
}
