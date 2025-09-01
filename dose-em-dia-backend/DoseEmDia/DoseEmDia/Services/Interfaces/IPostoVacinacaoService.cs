using DoseEmDia.Controllers.DTO;

namespace DoseEmDia.Services.Interfaces
{
    public interface IPostoVacinacaoService
    {
        Task<IReadOnlyList<PostoVacinacaoResponse>> BuscarPostosVacinaAsync(
            int usuarioId, CancellationToken ct = default);
    }
}