using DoseEmDia.Models.Localizacao;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoseEmDia.Models
{
    [Table("Pais")]
    public class Pais
    {
        [Key]
        public int IdPais { get; set; }

        public string Nome { get; set; } = string.Empty; 

        public string? Url { get; set; }

        public ICollection<Estado> Estados { get; set; } = new List<Estado>();
    }
}
