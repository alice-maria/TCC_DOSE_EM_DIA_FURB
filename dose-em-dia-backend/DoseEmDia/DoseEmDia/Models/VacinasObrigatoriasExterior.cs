namespace DoseEmDia.Models
{
    public class VacinasObrigatoriasExterior
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public int PaisId { get; set; }
        public Pais Pais { get; set; }
    }
}
