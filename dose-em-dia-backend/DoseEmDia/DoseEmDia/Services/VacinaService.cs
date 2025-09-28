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
            var nascimento = hoje.AddYears(-idadeAtual).Date;
            var rand = new Random(unchecked((int)DateTime.Now.Ticks));

            var elegiveis = _tabelaVacinas
                .Where(e =>
                    idadeAtual >= e.IdadeMinima &&
                    (!e.IdadeMaxima.HasValue || idadeAtual <= e.IdadeMaxima.Value) &&
                    (string.IsNullOrWhiteSpace(e.Sexo) || string.Equals(e.Sexo, sexo, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (!elegiveis.Any())
                elegiveis = _tabelaVacinas.ToList();

            var maxPorNome = elegiveis
                .GroupBy(e => e.Nome, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

            var countPorNome = maxPorNome.Keys.ToDictionary(k => k, _ => 0, StringComparer.OrdinalIgnoreCase);

            var maxPossivel = maxPorNome.Values.Sum();
            var alvoTotal = Math.Min(TOTAL, maxPossivel);

            var lista = new List<Vacina>();

            var esquemaBCG = elegiveis.FirstOrDefault(e => e.Nome.Equals("BCG", StringComparison.OrdinalIgnoreCase))
                  ?? _tabelaVacinas.FirstOrDefault(e => e.Nome.Equals("BCG", StringComparison.OrdinalIgnoreCase));

            if (esquemaBCG != null)
            {
                if (!maxPorNome.ContainsKey(esquemaBCG.Nome))
                    maxPorNome[esquemaBCG.Nome] = 1;
                if (!countPorNome.ContainsKey(esquemaBCG.Nome))
                    countPorNome[esquemaBCG.Nome] = 0;

                if (!lista.Any(v => v.Nome.Equals("BCG", StringComparison.OrdinalIgnoreCase)))
                {
                    var anoNascimento = hoje.Year - idadeAtual;
                    var dataAplicacaoBCG = new DateTime(anoNascimento, 1, 15);

                    var bcg = new Vacina
                    {
                        Nome = esquemaBCG.Nome,
                        IntervaloEntreDoses = esquemaBCG.Intervalo,
                        NumeroDoses = 1,
                        NumeroLote = rand.Next(100000, 999999),
                        DataAplicacao = dataAplicacaoBCG,
                        ValidadeMeses = esquemaBCG.ValidadeMeses,
                        Fabricante = esquemaBCG.Fabricante,
                        Status = StatusVacina.Aplicada
                    };

                    lista.Add(bcg);
                    countPorNome[esquemaBCG.Nome] = Math.Min(countPorNome[esquemaBCG.Nome] + 1, maxPorNome[esquemaBCG.Nome]);
                }
            }

            void tentarAdicionar(StatusVacina status, int quantidade)
            {
                if (quantidade <= 0) return;

                int adicionadas = 0;
                int tentativas = 0;
                int maxTentativas = quantidade * 20;

                while (adicionadas < quantidade && lista.Count < alvoTotal && tentativas < maxTentativas)
                {
                    tentativas++;

                    var candidatos = elegiveis
                        .Where(e => countPorNome.TryGetValue(e.Nome, out var cnt)
                                    ? cnt < maxPorNome[e.Nome]
                                    : true) 
                        .ToList();
                    if (candidatos.Count == 0) break;

                    var esquema = candidatos[rand.Next(candidatos.Count)];
                    var vacina = CriarVacinaParaStatus(esquema, status, hoje, nascimento, rand);
                    if (vacina == null) continue;

                    if (!countPorNome.ContainsKey(vacina.Nome))
                        countPorNome[vacina.Nome] = 0;
                    if (!maxPorNome.ContainsKey(vacina.Nome))
                        maxPorNome[vacina.Nome] = 1;

                    if (countPorNome[vacina.Nome] < maxPorNome[vacina.Nome])
                    {
                        lista.Add(vacina);
                        countPorNome[vacina.Nome]++;
                        adicionadas++;
                    }
                }
            }

            tentarAdicionar(StatusVacina.Aplicada, QT_APLICADAS);
            tentarAdicionar(StatusVacina.EmAtraso, QT_ATRASO);
            tentarAdicionar(StatusVacina.AVencer, QT_AVENCER);

            int faltantes = alvoTotal - lista.Count;
            tentarAdicionar(StatusVacina.Aplicada, faltantes);

            AplicarStatusComBaseNaValidade(lista);
            return lista;
        }

        private Vacina? CriarVacinaParaStatus(EsquemaVacinal esquema, StatusVacina statusDesejado, DateTime hoje, DateTime nascimento, Random rand)
        {
            var minDateIdade = nascimento.AddYears(esquema.IdadeMinima);
            var maxDateIdade = esquema.IdadeMaxima.HasValue
                ? nascimento.AddYears(esquema.IdadeMaxima.Value + 1).AddDays(-1)
                : hoje;

            var limiteDezAnos = hoje.AddYears(-10);
            var minDate = new DateTime(Math.Max(minDateIdade.Ticks, limiteDezAnos.Ticks));
            var maxDate = new DateTime(Math.Min(maxDateIdade.Ticks, hoje.Ticks));

            if (minDate > maxDate)
                return null;

            int validade = (esquema.ValidadeMeses > 0 && esquema.ValidadeMeses < 999) ? esquema.ValidadeMeses : 0;

            DateTime dataAplicacao;

            if (statusDesejado == StatusVacina.AVencer && validade > 0)
            {
                var inicioJanelaVenc = hoje.Date;
                var fimJanelaVenc = FimDoMes(hoje).Date;

                var minVencPossivel = minDate.AddMonths(validade).Date;

                var inicioVencEfetivo = new DateTime(Math.Max(inicioJanelaVenc.Ticks, minVencPossivel.Ticks));
                var fimVencEfetivo = fimJanelaVenc;

                if (inicioVencEfetivo > fimVencEfetivo)
                    return null; 

                var vencimentoAlvo = DataAleatoriaEntre(inicioVencEfetivo, fimVencEfetivo, rand);
                dataAplicacao = vencimentoAlvo.AddMonths(-validade);

                if (dataAplicacao < minDate) dataAplicacao = minDate;
                if (dataAplicacao > maxDate) dataAplicacao = maxDate;
            }
            else if (statusDesejado == StatusVacina.EmAtraso && validade > 0)
            {
                var minVenc = minDate.AddMonths(validade).Date;
                var maxVenc = hoje.AddDays(-1).Date;

                if (minVenc > maxVenc)
                    return null;

                var pisoVenc = new DateTime(Math.Max(minVenc.Ticks, hoje.AddYears(-1).Ticks));
                if (pisoVenc > maxVenc) pisoVenc = minVenc;

                var vencimentoAlvo = DataAleatoriaEntre(pisoVenc, maxVenc, rand);
                dataAplicacao = vencimentoAlvo.AddMonths(-validade);

                if (dataAplicacao < minDate) dataAplicacao = minDate;
                if (dataAplicacao > maxDate) dataAplicacao = maxDate;
            }
            else
            {
                var upperSugerido = hoje.AddDays(-31);
                var upper = new DateTime(Math.Min(maxDate.Ticks, upperSugerido.Ticks));
                if (upper < minDate) upper = maxDate; 

                dataAplicacao = DataAleatoriaEntre(minDate, upper, rand);
            }

            return new Vacina
            {
                Nome = esquema.Nome,
                IntervaloEntreDoses = esquema.Intervalo,
                NumeroDoses = 0,
                NumeroLote = rand.Next(100000, 999999),
                DataAplicacao = dataAplicacao,
                ValidadeMeses = esquema.ValidadeMeses,
                Fabricante = esquema.Fabricante,
                Status = statusDesejado
            };
        }

        private void AplicarStatusComBaseNaValidade(List<Vacina> vacinas)
        {
            var hoje = DateTime.Today;
            var anoAtual = hoje.Year;
            var mesAtual = hoje.Month;

            foreach (var vacina in vacinas)
            {
                int validade = vacina.ValidadeMeses ?? 0;

                if (validade <= 0 || validade >= 999)
                {
                    vacina.Status = StatusVacina.Aplicada;
                    continue;
                }

                var vencimento = vacina.DataAplicacao.AddMonths(validade).Date;

                if (vencimento < hoje)
                {
                    vacina.Status = StatusVacina.EmAtraso;
                }
                else if (vencimento.Year == anoAtual && vencimento.Month == mesAtual)
                {
                    vacina.Status = StatusVacina.AVencer;
                }
                else
                {
                    vacina.Status = StatusVacina.Aplicada;
                }
            }
        }

        private static DateTime FimDoMes(DateTime data)
        {
            var primeiroDiaMesSeguinte = new DateTime(data.Year, data.Month, 1).AddMonths(1);
            return primeiroDiaMesSeguinte.AddDays(-1);
        }

        private static DateTime DataAleatoriaEntre(DateTime inicio, DateTime fim, Random rand)
        {
            if (fim < inicio) return inicio;
            var range = (fim - inicio).Days;
            return inicio.AddDays(rand.Next(0, range + 1));
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
            new EsquemaVacinal { Nome = "dT (Dupla adulto)", IdadeMinima = 15, Intervalo = "Reforço a cada 10 anos", NumeroDoses = 1, ValidadeMeses = 120, Fabricante = "Butantan" },
            new EsquemaVacinal { Nome = "Febre Amarela", IdadeMinima = 9, Intervalo = "Dose única ou reforço a depender da região", NumeroDoses = 1, ValidadeMeses = 999, Fabricante = "Bio-Manguinhos" },
            new EsquemaVacinal { Nome = "Tríplice Viral (SCR)", IdadeMinima = 20, Intervalo = "2 doses se não vacinado na infância", NumeroDoses = 2, ValidadeMeses = 240, Fabricante = "Fiocruz" },
            new EsquemaVacinal { Nome = "Hepatite B", IdadeMinima = 20, Intervalo = "3 doses", NumeroDoses = 3, ValidadeMeses = 240, Fabricante = "Butantan" },
            new EsquemaVacinal { Nome = "Covid-19", IdadeMinima = 15, Intervalo = "2 ou 3 doses + reforços anuais", NumeroDoses = 3, ValidadeMeses = 12, Fabricante = "Pfizer" },

            // Idoso
            new EsquemaVacinal { Nome = "Pneumocócica 23-valente", IdadeMinima = 60, Intervalo = "Dose única ou esquema em 2 doses", NumeroDoses = 2, ValidadeMeses = 999, Fabricante = "GSK" },
            new EsquemaVacinal { Nome = "Influenza (trivalente)", IdadeMinima = 60, Intervalo = "Dose anual", NumeroDoses = 1, ValidadeMeses = 12, Fabricante = "Butantan" },
            new EsquemaVacinal { Nome = "Covid-19", IdadeMinima = 60, Intervalo = "Reforço a cada 6 meses", NumeroDoses = 4, ValidadeMeses = 6, Fabricante = "Pfizer" },
        };

        public class EsquemaVacinal
        {
            public string Nome { get; set; } = default!;
            public int IdadeMinima { get; set; } // em anos
            public int? IdadeMaxima { get; set; } // null = sem limite
            public string? Sexo { get; set; } // "F", "M" ou null
            public string Intervalo { get; set; } = default!;
            public int NumeroDoses { get; set; }
            public int ValidadeMeses { get; set; }
            public string Fabricante { get; set; } = default!;
        }
    }
}
