using DoseEmDia.Models.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace DoseEmDia.Controllers
{
    [ApiController]
    [Route("api/vacinas")]
    public class VacinaController : ControllerBase
    {
        private readonly VacinaService _vacinaService;

        public VacinaController(VacinaService vacinaService)
        {
            _vacinaService = vacinaService;
        }

        [HttpGet("listaVacinas/{cpf}")]
        public async Task<IActionResult> ObterVacinasPorCpf(string cpf)
        {
            try
            {
                var vacinas = await _vacinaService.ObterVacinasPorCpf(cpf);
                return Ok(vacinas);
            }
            catch (UsuarioException ex)
            {
                return NotFound(ex.Message);
            }
            catch (VacinaException ex)
            {
                return NotFound(ex.Message);
            }
            catch
            {
                return StatusCode(500, "Erro interno ao buscar vacinas.");
            }
        }
    }
}
