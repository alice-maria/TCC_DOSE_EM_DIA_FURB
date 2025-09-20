namespace DoseEmDia.Models.Localizacao
{
    public class Estado
    {
        public long IdEstado { get; set; }
        public string Nome { get; set; } = default!;
        public string Uf { get; set; } = default!;
        public long PaisId { get; set; }
        public Pais Pais { get; set; } = default!;

        public ICollection<Cidade> Cidades { get; set; } = new List<Cidade>();
    }
}
