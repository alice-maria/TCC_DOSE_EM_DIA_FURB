using DoseEmDia.Controllers.DTO;
using DoseEmDia.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DoseEmDia.Controllers
{
    [ApiController]
    [Route("api/localizacao")]
    public class PostoVacinacaoLocController : ControllerBase
    {
        private readonly IPostoVacinacaoService _service;

        public PostoVacinacaoLocController(IPostoVacinacaoService service)
        {
            _service = service;
        }

        [HttpGet("buscar-postos")]
        [ProducesResponseType(typeof(List<PostoVacinacaoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BuscarPostosVacina([FromQuery] int usuarioId, CancellationToken ct = default)
        {
            if (usuarioId <= 0)
                return BadRequest(Problem(title: "Parâmetro inválido", detail: "usuarioId deve ser > 0."));

            try
            {
                var locais = await _service.BuscarPostosVacinaAsync(usuarioId, ct);
                return Ok(locais?.ToList() ?? new List<PostoVacinacaoResponse>());
            }
            catch (OperationCanceledException)
            {
                return Ok(new List<PostoVacinacaoResponse>());
            }
            catch (RateLimitExceededException ex)
            {
                return StatusCode(StatusCodes.Status429TooManyRequests,
                    Problem(title: "Limite de requisições atingido", detail: ex.Message));
            }
            catch (InvalidOperationException ex) // erros de entrada/configuração
            {
                return BadRequest(Problem(title: "Solicitação inválida", detail: ex.Message));
            }
            catch (HttpRequestException ex) // falha na HERE
            {
                return StatusCode(StatusCodes.Status502BadGateway,
                    Problem(title: "Falha ao consultar provedor de mapa", detail: ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    Problem(title: "Erro interno", detail: ex.Message));
            }
        }
    }

    public sealed class RateLimitExceededException : Exception
    {
        public RateLimitExceededException(string message) : base(message) { }
    }
}
