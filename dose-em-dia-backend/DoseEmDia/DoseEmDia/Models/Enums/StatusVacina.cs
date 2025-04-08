namespace DoseEmDia.Models.Enums
{
    public enum StatusVacina
    {
        Aplicada,
        AVencer,
        EmAtraso
    }

    public static class StatusVacinaExtensions
    {
        public static string ToDisplayString(this StatusVacina status)
        {
            return status switch
            {
                StatusVacina.Aplicada => "Aplicada",
                StatusVacina.AVencer => "A vencer",
                StatusVacina.EmAtraso => "Em atraso",
                _ => "Desconhecido"
            };
        }
    }


}
