using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoseEmDia.Models.db;
using DoseEmDia.Models.Exceptions;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using DoseEmDia.Models.Enums;

namespace DoseEmDia.Controllers
{
    [ApiController]
    [Route("api/comprovante")]
    public class ComprovanteController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ComprovanteController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{usuarioId}/gerarComprovante")]
        public async Task<IActionResult> GerarPdfComprovante(int usuarioId)
        {
            var usuario = await _context.Usuario
                .Include(u => u.Endereco)
                .Include(u => u.Vacinas)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
                throw new UsuarioException.UsuarioNaoEncontradoException(usuarioId);

            var vacinas = usuario.Vacinas
                .Where(v => v.Status == StatusVacina.Aplicada)
                .OrderByDescending(v => v.DataAplicacao)
                .ToList();

            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Título
            document.Add(new Paragraph("Certificado de Vacinação")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBold().SetFontSize(16));

            document.Add(new Paragraph($"Nome: {usuario.Nome}"));
            document.Add(new Paragraph($"CPF: {usuario.CPF}"));
            document.Add(new Paragraph($"E-mail: {usuario.Email}"));
            document.Add(new Paragraph($"Telefone: {usuario.Telefone}"));
            document.Add(new Paragraph(" "));

            // Lista de vacinas
            foreach (var vacina in vacinas)
            {
                var paragrafoVacina = new Paragraph()
                    .Add(new Text($"Vacina: {vacina.Nome}").SetBold())
                    .Add($"\nAplicada em: {vacina.DataAplicacao:dd/MM/yyyy}")
                    .Add($"\nFabricante: {vacina.Fabricante}")
                    .Add($"\nLote: {vacina.NumeroLote}")
                    .SetMarginBottom(15);

                document.Add(paragrafoVacina);
            }

            // Rodapé
            document.Add(new Paragraph($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}")
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetFontSize(9));

            document.Close();

            return File(stream.ToArray(), "application/pdf", "comprovante-vacinacao.pdf");
        }
    }
}
