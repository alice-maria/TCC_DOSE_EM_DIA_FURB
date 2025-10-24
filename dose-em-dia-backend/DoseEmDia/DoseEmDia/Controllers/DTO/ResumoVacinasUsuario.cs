namespace DoseEmDia.Controllers.DTO
{
    public sealed record VacinaElegivel(string Nome, int IdadeMinima, int? IdadeMaxima, string? Sexo, string Intervalo, int NumeroDoses);
    public sealed record ResumoVacinasUsuario(int UsuarioId, IReadOnlyList<string> VacinasJaVinculadas, IReadOnlyList<VacinaElegivel> VacinasElegiveis, IReadOnlyList<VacinaElegivel> VacinasFaltantes);
}
