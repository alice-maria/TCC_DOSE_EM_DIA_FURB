using DoseEmDia.Models.Localizacao;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoseEmDia.Models
{
    [Table("Endereco")]
    public class Endereco
    {
        [Key]
        [Column("IdEndereco")]
        public int IdEndereco { get; set; }

        public long CepId { get; set; }
        public Cep Cep { get; set; } = null!;

        [MaxLength(255)]
        public string? Logradouro { get; set; }   

        [Required]
        [MaxLength(20)]
        public string Numero { get; set; } = null!; 

        public Endereco() { }

        public Endereco(int cepId, string numero, string? logradouro = null)
        {
            CepId = cepId;
            Numero = numero;
            Logradouro = logradouro;
        }
    }
}
