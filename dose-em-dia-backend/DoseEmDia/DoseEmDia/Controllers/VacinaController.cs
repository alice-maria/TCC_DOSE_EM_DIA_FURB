using DoseEmDia.Models;
using DoseEmDia.Models.db;
using DoseEmDia.Models.Enums;
using iText.Commons.Actions.Contexts;
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

            [HttpGet("listaVacinas/{cpf}")]
            public async Task<IActionResult> ObterVacinasPorCpf(string cpf)
            {
                var usuario = await _context.Usuario
                .Include(u => u.Vacinas)
                .FirstOrDefaultAsync(u => u.CPF == cpf);

                if (usuario == null)
                    return NotFound("Usuário não encontrado.");

                var vacinasOrdenadas = usuario.Vacinas
                .OrderByDescending(v =>
                    v.Status == StatusVacina.EmAtraso ? 1 :
                    v.Status == StatusVacina.AVencer ? 2 :
                    3)
                .ThenBy(v => v.DataAplicacao)
                .ToList();

                return Ok(vacinasOrdenadas);
            }

        }
    }
}
