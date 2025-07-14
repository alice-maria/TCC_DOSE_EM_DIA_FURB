using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using DoseEmDia.Models.db;
using DoseEmDia.Models.Enums;
using DoseEmDia.Models.Exceptions;

namespace DoseEmDia.Controllers
{
    public class ComprovanteService
    {
        private readonly ApplicationDbContext _context;

        public ComprovanteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(byte[] conteudoPdf, string nomeArquivo)> GerarPdfComprovante(int usuarioId)
        {
            var usuario = await _context.Usuario
                .Include(u => u.Endereco)
                .Include(u => u.Vacinas)
                .FirstOrDefaultAsync(u => u.IdUser == usuarioId);

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
            gfx.DrawString($"CPF: {FormatCPF(usuario.CPF)}", fontBold, XBrushes.Blue, 40, y); y += 20;
            gfx.DrawString($"E-mail: {usuario.Email}", fontBold, XBrushes.Blue, 40, y); y += 20;
            gfx.DrawString($"Telefone: {FormatTelefone(usuario.Telefone)}", fontBold, XBrushes.Blue, 40, y); y += 25;

            // Logo do sistema
            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imagens", "logo.png");
            if (System.IO.File.Exists(logoPath))
            {
                var image = XImage.FromFile(logoPath);
                double logoWidth = 140;
                double logoHeight = 120;
                double marginRight = 40;
                double logoX = page.Width - logoWidth - marginRight;
                double logoY = 20;

                gfx.DrawImage(image, logoX, logoY, logoWidth, logoHeight);
            }

            // Linha divisória
            gfx.DrawLine(XPens.Black, 40, y, page.Width - 40, y);
            y += 30;

            // Título centralizado
            gfx.DrawString("Certificado de Vacinação", fontTitle, XBrushes.Black,
                new XRect(0, y, page.Width, 20), XStringFormats.TopCenter);
            y += 20;
            gfx.DrawString($"Gerado em: {DateTime.Now:dd/MM/yyyy} às {DateTime.Now:HH:mm}", fontSmall, XBrushes.Black,
                new XRect(0, y, page.Width, 20), XStringFormats.TopCenter);
            y += 30;

            // Vacinas
            foreach (var vacina in vacinas)
            {
                // Caminho do ícone
                var iconePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imagens", "check-icon.png");
                if (System.IO.File.Exists(iconePath))
                {
                    var icone = XImage.FromFile(iconePath);
                    gfx.DrawImage(icone, 45, y, 16, 16); // ⬅️ ajuste a posição e tamanho como desejar
                }
                gfx.DrawString($"{vacina.Nome} - {vacina.NumeroDoses}", fontBold, XBrushes.Black, 70, y); y += 20;
                gfx.DrawString($"Aplicada em: {vacina.DataAplicacao:dd/MM/yyyy}", fontRegular, XBrushes.Black, 70, y); y += 25;

                if (y > page.Height - 100)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 40;
                }
            }

            //Rodapé
            var fontRodape = new XFont("Verdana", 8, XFontStyle.Italic);

            var linha1 = "Este documento foi gerado automaticamente pelo sistema Dose em Dia com base nos registros internos de vacinação.";
            var linha2 = "Trata-se de um comprovante informativo, sem validade jurídica, e não substitui certificados oficiais emitidos por autoridades públicas de saúde.";

            gfx.DrawString(linha1, fontRodape, XBrushes.Gray,
                new XRect(40, page.Height - 40, page.Width - 80, 20), XStringFormats.Center);

            gfx.DrawString(linha2, fontRodape, XBrushes.Gray,
                new XRect(40, page.Height - 25, page.Width - 80, 20), XStringFormats.Center);

            document.Save(stream, false);
            var cpfLimpo = usuario.CPF.Replace(".", "").Replace("-", "");
            var nomeArquivo = $"comprovante-vacinacao_{cpfLimpo}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            return (stream.ToArray(), nomeArquivo);
        }

        private string FormatCPF(string cpf)
        {
            return Convert.ToUInt64(cpf).ToString(@"000\.000\.000\-00");
        }

        private string FormatTelefone(string telefone)
        {
            telefone = new string(telefone.Where(char.IsDigit).ToArray());
            if (telefone.Length == 11)
                return Convert.ToUInt64(telefone).ToString(@"\(00\) 00000\-0000");
            else if (telefone.Length == 10)
                return Convert.ToUInt64(telefone).ToString(@"\(00\) 0000\-0000");
            return telefone;
        }

    }
}
