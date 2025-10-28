using DoseEmDia.Services;
using Microsoft.AspNetCore.Mvc;

namespace DoseEmDia.Controllers
{
    [Route("api/paises")]
    [ApiController]
    public class VacinasPaisExteriorController : ControllerBase
    {
        private readonly PaisService _servicePais;

        public VacinasPaisExteriorController(PaisService servicePais)
        {
            _servicePais = servicePais;
        }

        [HttpGet("listaPaises")]
        public async Task<IActionResult> ListarPaisesVacinasObrigatorias()
        {
            try
            {
                var paises = await _servicePais.ListarPaisesVacinasObrigatorias();
                return Ok(paises);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro ao buscar países: " + ex.Message);
            }
        }
    }
}
