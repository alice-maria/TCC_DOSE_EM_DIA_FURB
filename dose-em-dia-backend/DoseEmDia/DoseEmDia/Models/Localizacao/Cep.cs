using System.ComponentModel.DataAnnotations;

namespace DoseEmDia.Models.Localizacao
{
    public class Cep
    {
        public long IdCep { get; set; }
        public string Codigo { get; set; } = default!; 
        public long CidadeId { get; set; }
        public Cidade Cidade { get; set; } = default!;

        [MaxLength(160)]
        public string? Bairro { get; set; }

        public ICollection<Endereco> Enderecos { get; set; } = new List<Endereco>();
    }
}
