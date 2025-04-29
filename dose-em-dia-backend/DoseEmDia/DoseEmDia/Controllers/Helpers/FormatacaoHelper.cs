namespace DoseEmDia.Controllers.Helpers
{
    public class FormatacaoHelper
    {
        public static string FormatarDistancia(int distanciaMetros)
        {
            if (distanciaMetros >= 1000)
                return $"{(distanciaMetros / 1000.0):0.0} km";
            else
                return $"{distanciaMetros} metros";
        }
    }
}
