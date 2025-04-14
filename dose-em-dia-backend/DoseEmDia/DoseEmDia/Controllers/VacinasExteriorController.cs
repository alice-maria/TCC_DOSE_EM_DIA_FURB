using DoseEmDia.Models;
using DoseEmDia.Models.db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoseEmDia.Controllers
{
    [Route("api/paises")]
    [ApiController]
    public class VacinasExteriorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VacinasExteriorController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("listaPaises")]
        public async Task<ActionResult<IEnumerable<Pais>>> ListarPaisesVacinasObrigatorias()
        {
            var paises = await _context.Paises.ToListAsync();
            return Ok(paises);
        }
    }
}
