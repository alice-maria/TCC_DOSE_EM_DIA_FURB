using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using DoseEmDia.Models.db;
using DoseEmDia.Models.Enums;
using DoseEmDia.Models.Exceptions;

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
            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);

            var fontRegular = new XFont("Verdana", 11, XFontStyle.Regular);
            var fontBold = new XFont("Verdana", 11, XFontStyle.Bold);
            var fontTitle = new XFont("Verdana", 13, XFontStyle.Bold);
            var fontSmall = new XFont("Verdana", 9, XFontStyle.Regular);

            int y = 40;

            // Dados do usuário
            gfx.DrawString($"{usuario.Nome}", fontBold, XBrushes.Black, 40, y); y += 20;
            gfx.DrawString($"CPF: {usuario.CPF}", fontBold, XBrushes.Blue, 40, y); y += 20;
            gfx.DrawString($"E-mail: {usuario.Email}", fontBold, XBrushes.Blue, 40, y); y += 20;
            gfx.DrawString($"Telefone: {usuario.Telefone}", fontBold, XBrushes.Blue, 40, y); y += 25;

            // Nome do sistema
            gfx.DrawString("Dose em dia", fontBold, XBrushes.Black, page.Width - 130, y - 65);

            // Linha divisória
            gfx.DrawLine(XPens.Black, 40, y, page.Width - 40, y);
            y += 30;

            // Título centralizado
            gfx.DrawString("Certificado de Vacinação", fontTitle, XBrushes.Black,
                new XRect(0, y, page.Width, 20), XStringFormats.TopCenter);
            y += 20;
            gfx.DrawString($"Gerado em: {DateTime.Now:dd/MM/yyyy às HH:mm}", fontSmall, XBrushes.Black,
                new XRect(0, y, page.Width, 20), XStringFormats.TopCenter);
            y += 30;

            // Vacinas
            foreach (var vacina in vacinas)
            {
                // ✔️ unicode: 2714
                gfx.DrawString("✔", new XFont("Arial", 16), XBrushes.Green, 45, y + 12);
                gfx.DrawString($"{vacina.Nome} - {vacina.Dose}", fontBold, XBrushes.Black, 70, y); y += 20;
                gfx.DrawString($"Aplicada em: {vacina.DataAplicacao:dd/MM/yyyy}", fontRegular, XBrushes.Black, 70, y); y += 25;

                if (y > page.Height - 100)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 40;
                }
            }

            document.Save(stream, false);
            return File(stream.ToArray(), "application/pdf", "comprovante-vacinacao.pdf");
        }
    }
}
