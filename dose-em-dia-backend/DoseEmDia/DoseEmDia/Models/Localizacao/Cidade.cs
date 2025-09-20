using System.Runtime.ConstrainedExecution;

namespace DoseEmDia.Models.Localizacao
{
    public class Cidade
    {
        public long IdCidade { get; set; }
        public string Nome { get; set; } = default!;
        public int EstadoId { get; set; }
        public Estado Estado { get; set; } = default!;

        public ICollection<Cep> Ceps { get; set; } = new List<Cep>();
    }
}
