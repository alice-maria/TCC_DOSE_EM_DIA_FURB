using DoseEmDia.Models;
using DoseEmDia.Models.db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoseEmDia.Controllers
{
    [Route("api/paises")]
    [ApiController]
    public class VacinasExteriorController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VacinasExteriorController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("listaPaises")]
        public async Task<IActionResult> ListarPaisesVacinasObrigatorias()
        {
            try
            {
                var paises = await _context.Paises
                    .OrderBy(p => p.Nome)
                    .ToListAsync();

                return Ok(paises);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro ao buscar países: " + ex.Message);
            }
        }
    }
}
