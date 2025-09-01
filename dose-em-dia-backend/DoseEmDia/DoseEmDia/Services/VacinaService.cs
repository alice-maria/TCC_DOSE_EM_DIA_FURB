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
                .OrderBy(v =>
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
            const int TOTAL = 15;
            const int QT_APLICADAS = 10;
            const int QT_ATRASO = 3;
            const int QT_AVENCER = 2;

            var hoje = DateTime.Today;
            var rand = new Random(unchecked((int)DateTime.Now.Ticks));
            var lista = new List<Vacina>();

            var elegiveis = _tabelaVacinas
                .Where(e =>
                    idadeAtual >= e.IdadeMinima &&
                    (!e.IdadeMaxima.HasValue || idadeAtual <= e.IdadeMaxima.Value) &&
                    (string.IsNullOrWhiteSpace(e.Sexo) || string.Equals(e.Sexo, sexo, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (!elegiveis.Any())
                elegiveis = _tabelaVacinas.ToList();

            void adicionar(StatusVacina status, int quantidade)
            {
                for (int i = 0; i < quantidade && lista.Count < TOTAL; i++)
                {
                    var esquema = elegiveis[rand.Next(elegiveis.Count)];
                    var vacina = CriarVacinaParaStatus(esquema, status, hoje, rand);
                    lista.Add(vacina);
                }
            }

            adicionar(StatusVacina.Aplicada, QT_APLICADAS);
            adicionar(StatusVacina.EmAtraso, QT_ATRASO);
            adicionar(StatusVacina.AVencer, QT_AVENCER);

            while (lista.Count < TOTAL)
            {
                var esquema = elegiveis[rand.Next(elegiveis.Count)];
                lista.Add(CriarVacinaParaStatus(esquema, StatusVacina.Aplicada, hoje, rand));
            }

            AplicarStatusComBaseNaValidade(lista);

            return lista;
        }

        private Vacina CriarVacinaParaStatus(EsquemaVacinal esquema, StatusVacina status, DateTime hoje, Random rand)
        {
            int validade = esquema.ValidadeMeses > 0 ? esquema.ValidadeMeses : 120;

            DateTime vencimentoAlvo = status switch
            {
                StatusVacina.EmAtraso => hoje.AddDays(-rand.Next(1, 366)),
                StatusVacina.AVencer => hoje.AddDays(rand.Next(1, 31)),
                _ => hoje.AddDays(rand.Next(31, 365)),
            };

            DateTime dataAplicacao = vencimentoAlvo.AddMonths(-validade);

            var vacina = new Vacina
            {
                Nome = esquema.Nome,
                IntervaloEntreDoses = esquema.Intervalo,
                NumeroDoses = 0,                        
                NumeroLote = rand.Next(100000, 999999),
                DataAplicacao = dataAplicacao,
                ValidadeMeses = esquema.ValidadeMeses,
                Fabricante = esquema.Fabricante,
                Status = status
            };

            return vacina;
        }

        private void AplicarStatusComBaseNaValidade(List<Vacina> vacinas)
        {
            foreach (var vacina in vacinas)
            {
                int validade = (vacina.ValidadeMeses ?? 0);

                if (validade <= 0) continue;

                var vencimento = vacina.DataAplicacao.AddMonths(validade);
                var diasRestantes = (vencimento - DateTime.Today).TotalDays;

                var statusCalculado = diasRestantes < 0
                    ? StatusVacina.EmAtraso
                    : (diasRestantes <= 30 ? StatusVacina.AVencer : StatusVacina.Aplicada);

                if (vacina.Status != statusCalculado)
                {
                    Console.WriteLine($"Aviso: {vacina.Nome} está marcada como {vacina.Status}, mas cálculo deu {statusCalculado}");
                }
            }
        }

        private readonly List<EsquemaVacinal> _tabelaVacinas = new()
        {
            new EsquemaVacinal { Nome = "BCG", IdadeMinima = 0, Intervalo = "Única ao nascer", NumeroDoses = 1, ValidadeMeses = 999, Fabricante = "Bio-Manguinhos" },
            new EsquemaVacinal { Nome = "Hepatite B", IdadeMinima = 0, Intervalo = "0, 1 e 6 meses", NumeroDoses = 3, ValidadeMeses = 240, Fabricante = "Butantan" },
            new EsquemaVacinal { Nome = "Penta", IdadeMinima = 0, Intervalo = "2, 4 e 6 meses", NumeroDoses = 3, ValidadeMeses = 6, Fabricante = "Serum Institute" },
            new EsquemaVacinal { Nome = "Poliomielite (VIP)", IdadeMinima = 0, Intervalo = "2, 4, 6 meses + reforços", NumeroDoses = 5, ValidadeMeses = 48, Fabricante = "Sanofi Pasteur" },
            new EsquemaVacinal { Nome = "Rotavírus", IdadeMinima = 0, IdadeMaxima = 1, Intervalo = "2 e 4 meses", NumeroDoses = 2, ValidadeMeses = 6, Fabricante = "GSK" },
            new EsquemaVacinal { Nome = "Pneumo 10", IdadeMinima = 0, Intervalo = "2, 4 meses + reforço 12 meses", NumeroDoses = 3, ValidadeMeses = 12, Fabricante = "Pfizer" },
            new EsquemaVacinal { Nome = "Meningo C", IdadeMinima = 0, Intervalo = "3, 5 meses + reforço 12 meses", NumeroDoses = 3, ValidadeMeses = 12, Fabricante = "Novartis" },
            new EsquemaVacinal { Nome = "Febre Amarela", IdadeMinima = 0, Intervalo = "9 meses + reforço aos 4 anos", NumeroDoses = 2, ValidadeMeses = 999, Fabricante = "Bio-Manguinhos" },
            new EsquemaVacinal { Nome = "Tríplice Viral (SCR)", IdadeMinima = 1, Intervalo = "12 e 15 meses", NumeroDoses = 2, ValidadeMeses = 120, Fabricante = "Fiocruz" },
            new EsquemaVacinal { Nome = "Tetraviral (SCRV)", IdadeMinima = 1, IdadeMaxima = 5, Intervalo = "15 meses", NumeroDoses = 1, ValidadeMeses = 120, Fabricante = "MSD" },
            new EsquemaVacinal { Nome = "DTP", IdadeMinima = 1, Intervalo = "15 meses + 4 anos", NumeroDoses = 2, ValidadeMeses = 48, Fabricante = "Butantan" },
            new EsquemaVacinal { Nome = "Hepatite A", IdadeMinima = 1, Intervalo = "15 meses", NumeroDoses = 1, ValidadeMeses = 999, Fabricante = "GSK" },
            new EsquemaVacinal { Nome = "Varicela", IdadeMinima = 1, Intervalo = "15 meses", NumeroDoses = 1, ValidadeMeses = 999, Fabricante = "MSD" },
            
            // Adolescente
            new EsquemaVacinal { Nome = "HPV quadrivalente", IdadeMinima = 9, IdadeMaxima = 14, Sexo = "F", Intervalo = "2 doses com 6 meses de intervalo", NumeroDoses = 2, ValidadeMeses = 120, Fabricante = "MSD" },
            new EsquemaVacinal { Nome = "HPV quadrivalente", IdadeMinima = 9, IdadeMaxima = 14, Sexo = "M", Intervalo = "2 doses com 6 meses de intervalo", NumeroDoses = 2, ValidadeMeses = 120, Fabricante = "MSD" },
            new EsquemaVacinal { Nome = "Meningocócica ACWY", IdadeMinima = 11, IdadeMaxima = 14, Intervalo = "Dose única", NumeroDoses = 1, ValidadeMeses = 999, Fabricante = "Sanofi" },
            
            // Adulto
            new EsquemaVacinal { Nome = "dT (Dupla adulto)", IdadeMinima = 10, Intervalo = "Reforço a cada 10 anos", NumeroDoses = 1, ValidadeMeses = 120, Fabricante = "Butantan" },
            new EsquemaVacinal { Nome = "Febre Amarela", IdadeMinima = 9, Intervalo = "Dose única ou reforço a depender da região", NumeroDoses = 1, ValidadeMeses = 999, Fabricante = "Bio-Manguinhos" },
            new EsquemaVacinal { Nome = "Tríplice Viral (SCR)", IdadeMinima = 20, Intervalo = "2 doses se não vacinado na infância", NumeroDoses = 2, ValidadeMeses = 240, Fabricante = "Fiocruz" },
            new EsquemaVacinal { Nome = "Hepatite B", IdadeMinima = 20, Intervalo = "3 doses", NumeroDoses = 3, ValidadeMeses = 240, Fabricante = "Butantan" },
            new EsquemaVacinal { Nome = "Covid-19", IdadeMinima = 6, Intervalo = "2 ou 3 doses + reforços anuais", NumeroDoses = 3, ValidadeMeses = 12, Fabricante = "Pfizer" },
            
            // Idoso
            new EsquemaVacinal { Nome = "Pneumocócica 23-valente", IdadeMinima = 60, Intervalo = "Dose única ou esquema em 2 doses", NumeroDoses = 2, ValidadeMeses = 999, Fabricante = "GSK" },
            new EsquemaVacinal { Nome = "Influenza (trivalente)", IdadeMinima = 60, Intervalo = "Dose anual", NumeroDoses = 1, ValidadeMeses = 12, Fabricante = "Butantan" },
            new EsquemaVacinal { Nome = "Covid-19", IdadeMinima = 60, Intervalo = "Reforço a cada 6 meses", NumeroDoses = 4, ValidadeMeses = 6, Fabricante = "Pfizer" },

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
            public string Fabricante { get; set; }
        }
    }
}
