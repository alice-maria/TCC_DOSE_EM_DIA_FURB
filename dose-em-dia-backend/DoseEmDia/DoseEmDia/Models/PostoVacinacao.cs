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

        public record HereDiscoverResponse(HereItem[]? Items);
        public record HereItem(
            string? Title,
            HereAddress? Address,
            HerePosition? Position,
            double? Distance,
            string? Id
        );
        public record HereAddress(string? Label, string? Street, string? District, string? City, string? State);
        public record HerePosition(double Lat, double Lng);

        public record PostoVacinacaoResponse
        {
            public string? Nome { get; set; }
            public string? EnderecoCompleto { get; set; }
            public double? DistanciaMetros { get; set; }
            public double? Lat { get; set; }
            public double? Lng { get; set; }
            public string? LinkGoogleMaps { get; set; }
        }

    }
}
