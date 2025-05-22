using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DoseEmDia.Models
{
    public class Pais
    {
        [Key]
        [Column("IdPais")]
        public int IdPais { get; set; }
        public string Nome { get; set; }
        public string Url { get; set; }
    }
}
