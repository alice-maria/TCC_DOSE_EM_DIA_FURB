using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DoseEmDia.Models
{
    public class Endereco
    {
        [Key]
        [Column("IdEndereco")]
        public int IdEndereco { get; set; }
        public string Logradouro { get; set; }
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
        public string CEP { get; set; }
        public string Pais { get; set; }

        public Endereco() { }

        public Endereco(string logradouro, string bairro, string cidade, string estado, string cep, string pais)
        {
            Logradouro = logradouro;
            Bairro = bairro;
            Cidade = cidade;
            Estado = estado;
            CEP = cep;
            Pais = pais;
        }
    }
}
