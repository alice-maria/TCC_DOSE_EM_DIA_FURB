using DoseEmDia.Controllers.DTO;

namespace DoseEmDia.Services.Geo
{
    public static class PostoAdapter
    {
        public static IReadOnlyList<PostoVacinacaoResponse> RespostaPostoVacinacao(IEnumerable<PostoMaisProximo> origem)
        {
            return origem.Select(x => new PostoVacinacaoResponse
            {
                Nome = x.Nome,
                EnderecoCompleto = x.Endereco,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                DistanciaMetros = x.DistanciaMetros ?? int.MaxValue,
                Distancia = x.DistanciaMetros is null
                    ? "—"
                    : (x.DistanciaMetros < 1000
                        ? $"{x.DistanciaMetros} m"
                        : $"{(x.DistanciaMetros.Value / 1000.0):0.0} km")
            }).ToList();
        }
    }
}