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

            var fontRegular = new XFont("Arial", 11, XFontStyle.Regular);
            var fontBold = new XFont("Arial", 11, XFontStyle.Bold);
            var fontTitle = new XFont("Arial", 13, XFontStyle.Bold);
            var fontSmall = new XFont("Arial", 9, XFontStyle.Regular);

            int y = 40;

            // Dados do usuário
            gfx.DrawString($"{usuario.Nome}", fontBold, XBrushes.Black, 40, y); y += 20;
            gfx.DrawString($"CPF: {FormatCPF(usuario.CPF)}", fontBold, XBrushes.Blue, 40, y); y += 20;
            gfx.DrawString($"E-mail: {usuario.Email}", fontBold, XBrushes.Blue, 40, y); y += 20;
            gfx.DrawString($"Telefone: {FormatTelefone(usuario.Telefone)}", fontBold, XBrushes.Blue, 40, y); y += 25;

            // Linha divisória
            gfx.DrawLine(XPens.Black, 40, y, page.Width - 40, y);
            y += 30;

            // Título centralizado
            gfx.DrawString("Certificado de Vacinação", fontTitle, XBrushes.Black,
                new XRect(0, y, page.Width, 20), XStringFormats.TopCenter);
            y += 20;
            gfx.DrawString($"Gerado em: {DateTime.Now.AddHours(-3):dd/MM/yyyy} às {DateTime.Now.AddHours(-3):HH:mm}", fontSmall, XBrushes.Black,
                new XRect(0, y, page.Width, 20), XStringFormats.TopCenter);
            y += 30;

            var fontLogo = new XFont("Arial", 16, XFontStyle.Bold);
            var brushLogo = new XSolidBrush(XColors.OrangeRed);

            string textoLogo = "Dose em Dia";
            var tamanho = gfx.MeasureString(textoLogo, fontLogo);
            double logoX = page.Width - tamanho.Width - 40; // margem direita
            double logoY = 30;

            gfx.DrawString(textoLogo, fontLogo, brushLogo, logoX, logoY);

            // Vacinas
            foreach (var vacina in vacinas)
            {
                gfx.DrawString($"✓ {vacina.Nome}", fontBold, XBrushes.Black, 70, y); y += 20;
                gfx.DrawString($"Aplicada em: {vacina.DataAplicacao:dd/MM/yyyy}", fontRegular, XBrushes.Black, 70, y); y += 25;

                if (y > page.Height - 100)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 40;
                }
            }

            //Rodapé
            var fontRodape = new XFont("Arial", 8, XFontStyle.Italic);

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

        private string FormatCPF(string? cpf)
        {
            var digits = new string((cpf ?? "").Where(char.IsDigit).ToArray());
            if (digits.Length != 11) return cpf ?? "";
            return Convert.ToUInt64(digits).ToString(@"000\.000\.000\-00");
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
