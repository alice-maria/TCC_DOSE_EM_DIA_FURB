using DoseEmDia.Models.Localizacao;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoseEmDia.Models
{
    [Table("Pais")]
    public class Pais
    {
        [Key]
        [Column("IdPais")]
        public int IdPais { get; set; }

        [Required]
        [MaxLength(120)]
        public string Nome { get; set; } = string.Empty; 

        [MaxLength(300)]
        [Url]
        public string? Url { get; set; }

        public ICollection<Estado> Estados { get; set; } = new List<Estado>();
    }
}
