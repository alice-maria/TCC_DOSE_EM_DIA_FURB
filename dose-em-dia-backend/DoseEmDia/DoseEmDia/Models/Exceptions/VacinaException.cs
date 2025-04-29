namespace DoseEmDia.Models.Exceptions
{
    public class VacinaException : Exception
    {
        public VacinaException(string mensagem): base(mensagem) { }
        public static VacinaException NenhumaVacinaEncontrada(int usuarioId) =>
            new VacinaException($"Nenhuma vacina encontrada para o usuário {usuarioId}.");
    }
}
