namespace DoseEmDia.Models.Exceptions
{
    public class CepNaoEncontradoException : Exception
    {
        public string CodigoCep { get; }

        public CepNaoEncontradoException(string codigoCep)
            : base($"CEP '{codigoCep}' não foi encontrado.")
        {
            CodigoCep = codigoCep;
        }
    }
}
