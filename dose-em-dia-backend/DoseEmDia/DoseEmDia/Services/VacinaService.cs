using DoseEmDia.Models;
using DoseEmDia.Models.db;
using DoseEmDia.Models.Enums;
using DoseEmDia.Models.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace DoseEmDia.Controllers
{
    public class VacinaService
    {
        private readonly ApplicationDbContext _context;

        public VacinaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Vacina>> ObterVacinasPorCpf(string cpf)
        {
            var usuario = await _context.Usuario
                .Include(u => u.Vacinas)
                .FirstOrDefaultAsync(u => u.CPF == cpf);

            if (usuario == null)
                throw UsuarioException.UsuarioNaoEncontradoPorCpf(cpf);

            if (usuario.Vacinas == null || !usuario.Vacinas.Any())
                throw VacinaException.NenhumaVacinaEncontrada(usuario.IdUser);

            return usuario.Vacinas
                .OrderByDescending(v =>
                    v.Status == StatusVacina.EmAtraso ? 1 :
                    v.Status == StatusVacina.AVencer ? 2 :
                    3)
                .ThenBy(v => v.DataAplicacao)
                .ToList();
        }

        public async Task<List<Vacina>> GerarEVincularVacinas(int usuarioId, int idade, string? sexo)
        {
            var vacinas = GerarHistoricoVacinalPorIdade(idade, sexo);
            foreach (var vacina in vacinas)
            {
                vacina.UsuarioId = usuarioId;
                _context.Vacina.Add(vacina);
            }
            await _context.SaveChangesAsync();
            return vacinas;
        }

        private List<Vacina> GerarHistoricoVacinalPorIdade(int idadeAtual, string? sexo)
        {
            var hoje = DateTime.Today;
            var rand = new Random();
            var lista = new List<Vacina>();

            foreach (var esquema in _tabelaVacinas)
            {
                if (idadeAtual < esquema.IdadeMinima)
                    continue;

                if (esquema.IdadeMaxima.HasValue && idadeAtual > esquema.IdadeMaxima.Value)
                    continue;

                if (!string.IsNullOrWhiteSpace(esquema.Sexo) && sexo != esquema.Sexo)
                    continue;

                var idadeAplicacao = esquema.IdadeMinima + rand.Next(0, Math.Max(1, idadeAtual - esquema.IdadeMinima + 1));
                var dataAplicacao = hoje.AddYears(-idadeAplicacao).AddMonths(rand.Next(-6, 6));

                lista.Add(new Vacina
                {
                    Nome = esquema.Nome,
                    IntervaloEntreDoses = esquema.Intervalo,
                    NumeroDoses = esquema.NumeroDoses,
                    NumeroLote = rand.Next(100000, 999999),
                    DataAplicacao = dataAplicacao,
                    ValidadeMeses = esquema.ValidadeMeses
                });
            }

            AplicarStatusComBaseNaValidade(lista);
            return lista;
        }

        private void AplicarStatusComBaseNaValidade(List<Vacina> vacinas)
        {
            foreach (var vacina in vacinas)
            {
                if ((vacina.ValidadeMeses ?? 0) <= 0)
                {
                    vacina.Status = StatusVacina.Aplicada; // vacinas de dose única ou sem validade
                    continue;
                }

                var vencimento = vacina.DataAplicacao.AddMonths(vacina.ValidadeMeses ?? 0);
                var diasRestantes = (vencimento - DateTime.Today).TotalDays;

                if (diasRestantes < 0)
                    vacina.Status = StatusVacina.EmAtraso;
                else if (diasRestantes <= 30)
                    vacina.Status = StatusVacina.AVencer;
                else
                    vacina.Status = StatusVacina.Aplicada;
            }
        }

        private readonly List<EsquemaVacinal> _tabelaVacinas = new()
        {
            // --- Criança ---
            new EsquemaVacinal { Nome = "BCG", IdadeMinima = 0, Intervalo = "Única ao nascer", NumeroDoses = 1, ValidadeMeses = 999 },
            new EsquemaVacinal { Nome = "Hepatite B", IdadeMinima = 0, Intervalo = "0, 1 e 6 meses", NumeroDoses = 3, ValidadeMeses = 240 },
            new EsquemaVacinal { Nome = "Penta", IdadeMinima = 0, Intervalo = "2, 4 e 6 meses", NumeroDoses = 3, ValidadeMeses = 6 },
            new EsquemaVacinal { Nome = "Poliomielite (VIP)", IdadeMinima = 0, Intervalo = "2, 4, 6 meses + reforços", NumeroDoses = 5, ValidadeMeses = 48 },
            new EsquemaVacinal { Nome = "Rotavírus", IdadeMinima = 0, IdadeMaxima = 1, Intervalo = "2 e 4 meses", NumeroDoses = 2, ValidadeMeses = 6 },
            new EsquemaVacinal { Nome = "Pneumo 10", IdadeMinima = 0, Intervalo = "2, 4 meses + reforço 12 meses", NumeroDoses = 3, ValidadeMeses = 12 },
            new EsquemaVacinal { Nome = "Meningo C", IdadeMinima = 0, Intervalo = "3, 5 meses + reforço 12 meses", NumeroDoses = 3, ValidadeMeses = 12 },
            new EsquemaVacinal { Nome = "Febre Amarela", IdadeMinima = 0, Intervalo = "9 meses + reforço aos 4 anos", NumeroDoses = 2, ValidadeMeses = 999 },
            new EsquemaVacinal { Nome = "Tríplice Viral (SCR)", IdadeMinima = 1, Intervalo = "12 e 15 meses", NumeroDoses = 2, ValidadeMeses = 120 },
            new EsquemaVacinal { Nome = "Tetraviral (SCRV)", IdadeMinima = 1, IdadeMaxima = 5, Intervalo = "15 meses", NumeroDoses = 1, ValidadeMeses = 120 },
            new EsquemaVacinal { Nome = "DTP", IdadeMinima = 1, Intervalo = "15 meses + 4 anos", NumeroDoses = 2, ValidadeMeses = 48 },
            new EsquemaVacinal { Nome = "Hepatite A", IdadeMinima = 1, Intervalo = "15 meses", NumeroDoses = 1, ValidadeMeses = 999 },
            new EsquemaVacinal { Nome = "Varicela", IdadeMinima = 1, Intervalo = "15 meses", NumeroDoses = 1, ValidadeMeses = 999 },
        
            // --- Adolescente ---
            new EsquemaVacinal { Nome = "HPV quadrivalente", IdadeMinima = 9, IdadeMaxima = 14, Sexo = "F", Intervalo = "2 doses com 6 meses de intervalo", NumeroDoses = 2, ValidadeMeses = 120 },
            new EsquemaVacinal { Nome = "HPV quadrivalente", IdadeMinima = 9, IdadeMaxima = 14, Sexo = "M", Intervalo = "2 doses com 6 meses de intervalo", NumeroDoses = 2, ValidadeMeses = 120 },
            new EsquemaVacinal { Nome = "Meningocócica ACWY", IdadeMinima = 11, IdadeMaxima = 14, Intervalo = "Dose única", NumeroDoses = 1, ValidadeMeses = 999 },
        
            // --- Adulto ---
            new EsquemaVacinal { Nome = "dT (Dupla adulto)", IdadeMinima = 10, Intervalo = "Reforço a cada 10 anos", NumeroDoses = 1, ValidadeMeses = 120 },
            new EsquemaVacinal { Nome = "Febre Amarela", IdadeMinima = 9, Intervalo = "Dose única ou reforço a depender da região", NumeroDoses = 1, ValidadeMeses = 999 },
            new EsquemaVacinal { Nome = "Tríplice Viral (SCR)", IdadeMinima = 20, Intervalo = "2 doses se não vacinado na infância", NumeroDoses = 2, ValidadeMeses = 240 },
            new EsquemaVacinal { Nome = "Hepatite B", IdadeMinima = 20, Intervalo = "3 doses", NumeroDoses = 3, ValidadeMeses = 240 },
            new EsquemaVacinal { Nome = "Covid-19", IdadeMinima = 6, Intervalo = "2 ou 3 doses + reforços anuais", NumeroDoses = 3, ValidadeMeses = 12 },
        
            // --- Idoso ---
            new EsquemaVacinal { Nome = "Pneumocócica 23-valente", IdadeMinima = 60, Intervalo = "Dose única ou esquema em 2 doses", NumeroDoses = 2, ValidadeMeses = 999 },
            new EsquemaVacinal { Nome = "Influenza (trivalente)", IdadeMinima = 60, Intervalo = "Dose anual", NumeroDoses = 1, ValidadeMeses = 12 },
            new EsquemaVacinal { Nome = "Covid-19", IdadeMinima = 60, Intervalo = "Reforço a cada 6 meses", NumeroDoses = 4, ValidadeMeses = 6 }
        };

        public class EsquemaVacinal
        {
            public string Nome { get; set; }
            public int IdadeMinima { get; set; } // em anos
            public int? IdadeMaxima { get; set; } // null = sem limite
            public string? Sexo { get; set; } // "F", "M" ou null
            public string Intervalo { get; set; }
            public int NumeroDoses { get; set; }
            public int ValidadeMeses { get; set; }
        }

    }
}
