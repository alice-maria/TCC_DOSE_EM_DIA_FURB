using DoseEmDia.Models.Enums;

namespace DoseEmDia.Models
{
    public class Vacina
    {
        public string Nome { get; set; }
        public string Fabricante { get; set; }
        public int NumeroDoses { get; set; }
        public string IntervaloEntreDoses { get; set; }
        public DateTime DataAplicacao { get; set; }
        public StatusVacina Status { get; set; }
        public int UsuarioId { get; set; }

        public Vacina(string nome, string fabricante,int numeroDoses, string intervaloEntreDoses, DateTime dataAplicacao, StatusVacina status)
        {
            Nome = nome;
            Fabricante = fabricante;
            NumeroDoses = numeroDoses;
            IntervaloEntreDoses = intervaloEntreDoses;
            DataAplicacao = dataAplicacao;
            Status = status;
        }
    }
}
