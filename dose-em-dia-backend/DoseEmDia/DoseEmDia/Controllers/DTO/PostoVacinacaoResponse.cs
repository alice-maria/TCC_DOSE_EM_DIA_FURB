namespace DoseEmDia.Controllers.DTO
{
    public class PostoVacinacaoResponse
    {
        public string Nome { get; set; } = "";
        public string EnderecoCompleto { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int DistanciaMetros { get; set; }
        public string Distancia { get; set; } = "";
        public string LinkGoogleMaps { get; set; } = "";
    }

}
