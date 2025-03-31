namespace DoseEmDia.Models
{
    public class Vacina
    {
        public string Nome { get; set; }
        public string Fabricante { get; set; }
        public int NumeroDoses { get; set; }
        public string IntervaloEntreDoses { get; set; }

        public Vacina(string nome, string fabricante,int numeroDoses, string intervaloEntreDoses)
        {
            Nome = nome;
            Fabricante = fabricante;
            NumeroDoses = numeroDoses;
            IntervaloEntreDoses = intervaloEntreDoses;
        }
    }
}
