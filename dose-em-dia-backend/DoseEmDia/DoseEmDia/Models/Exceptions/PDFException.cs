namespace DoseEmDia.Models.Exceptions
{
    public class PDFException : Exception
    {
        public PDFException(string mensagem) : base(mensagem) { }

        public static PDFException SemVacinasParaGerar() =>
            new PDFException("Nenhuma vacina aplicada encontrada para gerar o comprovante.");

        public static PDFException ErroGeracaoPDF(string mensagem) =>
            new PDFException($"Erro ao gerar o arquivo PDF: {mensagem}");
    }
}