using DoseEmDia.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoseEmDia.Models
{
    public class Vacina
    {
        [Key]
        [Column("IdVacina")]
        public int IdVacina { get; set; }
        public string Nome { get; set; }
        public string Fabricante { get; set; }
        public int NumeroDoses { get; set; }
        public int NumeroLote { get; set; }
        public string IntervaloEntreDoses { get; set; }
        public DateTime DataAplicacao { get; set; }
        public StatusVacina Status { get; set; }
        public Usuario Usuario { get; set; }
        public int UsuarioId { get; set; }
        public int? ValidadeMeses { get; set; }

        public Vacina(string nome, string fabricante,int numeroDoses, int numeroLote, string intervaloEntreDoses, DateTime dataAplicacao, StatusVacina status)
        {
            Nome = nome;
            Fabricante = fabricante;
            NumeroDoses = numeroDoses;
            NumeroLote = numeroLote;
            IntervaloEntreDoses = intervaloEntreDoses;
            DataAplicacao = dataAplicacao;
            Status = status;
        }

        public Vacina() { }
    }
}
