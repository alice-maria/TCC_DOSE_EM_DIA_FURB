using DoseEmDia.Models;
using Microsoft.AspNetCore.Mvc;

namespace DoseEmDia.Controllers
{
    public class VacinaController : Controller
    {
        [ApiController]
        [Route("api/[controller]")]
        public class VacinasController : ControllerBase
        {
            [HttpGet("{usuarioId}")]
            public IActionResult GetVacinas(string usuarioId)
            {
                
            }

            public string ObterStatus(DateTime dataAplicacao)
            {
                if (dataAplicacao < DateTime.Today)
                    return "Aplicada";

                var diasRestantes = (dataAplicacao - DateTime.Today).TotalDays;

                if (diasRestantes <= 7)
                    return "A vencer";

                return "Em atraso"; // se a data passou ou não foi aplicada
            }
        }

    }
}
