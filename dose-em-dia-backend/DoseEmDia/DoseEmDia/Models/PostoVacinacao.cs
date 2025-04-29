namespace DoseEmDia.Models
{
    public class PostoVacinacao
    {
        public string Nome { get; set; }
        public Endereco Endereco { get; set; }
        public int DistanciaMetros { get; set; }
        public string LinkGoogleMaps { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public PostoVacinacao() { }
    }
}
