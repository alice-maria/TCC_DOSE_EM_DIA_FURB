using DoseEmDia.Models.db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace DoseEmDia.Controllers
{
    public class VacinaController : Controller
    {
        [ApiController]
        [Route("api/[controller]")]

        public class VacinasController : ControllerBase
        {
            private readonly ApplicationDbContext _context;

            public VacinasController(ApplicationDbContext context)
            {
                _context = context;
            }

            [HttpGet("{usuarioId}")]
            public async Task<IActionResult> GetVacinas(int usuarioId)
            {
                var vacinas = await _context.Vacina
                    .Where(v => v.UsuarioId == usuarioId)
                    .ToListAsync();

                return Ok(vacinas);
            }

            ///Verificar tempo de cada vacina

            public string ObterStatus(DateTime dataAplicacao)
            {
                /*if (dataAplicacao < DateTime.Today)
                    return "Aplicada";

                var diasRestantes = (dataAplicacao - DateTime.Today).TotalDays;

                if (diasRestantes <= 7)
                    return "A vencer";

                return "Em atraso";*/

                return null;
            }
        }

    }
}
