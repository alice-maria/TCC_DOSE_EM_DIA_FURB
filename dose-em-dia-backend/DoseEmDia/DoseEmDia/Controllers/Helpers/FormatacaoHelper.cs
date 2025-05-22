using System.Text.RegularExpressions;

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

        public static string FormatarPais(string pais)
        {
            if (pais.Contains("Brasil") || pais.Contains("br"))
            {
                return "BR";
            }

            return "estamos vendo se estrangeiros podem ou não criar conta no projeto";
        }

        public static string FormataCPF(string cpf)
        {
            string rx = @"[^0-9]";
            return cpf = Regex.Replace(cpf, rx, "");
        }

        public static string FormataTelefone(string telefone)
        {
            string rx = @"[^0-9]";
            return telefone = Regex.Replace(telefone, rx, "");
        }

        public static string FormataCEP(string cep)
        {
            string rx = @"[^0-9]";
            return cep = Regex.Replace(cep, rx, "");
        }
    }
}
