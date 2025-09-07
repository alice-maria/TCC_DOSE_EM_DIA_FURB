using System.Globalization;
using DoseEmDia.Controllers.DTO;

namespace DoseEmDia.Services.Geo
{
    public static class PostoAdapter
    {
        public static IReadOnlyList<PostoVacinacaoResponse> ToPostoVacinacaoResponse(
            IEnumerable<NearPlaceResult> src)
        {
            return src.Select(x => new PostoVacinacaoResponse
            {
                Nome = x.Name,
                EnderecoCompleto = x.Address,
                Latitude = x.Latitude,
                Longitude = x.Longitude,
                DistanciaMetros = x.DistanceMeters ?? int.MaxValue,
                Distancia = x.DistanceMeters is null
                    ? "—"
                    : (x.DistanceMeters < 1000
                        ? $"{x.DistanceMeters} m"
                        : $"{(x.DistanceMeters.Value / 1000.0):0.0} km"),
                LinkGoogleMaps =
                    $"https://www.google.com/maps/search/?api=1&query={x.Latitude.ToString(CultureInfo.InvariantCulture)},{x.Longitude.ToString(CultureInfo.InvariantCulture)}"
            }).ToList();
        }
    }
}
