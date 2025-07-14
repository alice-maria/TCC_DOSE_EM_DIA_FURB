using DoseEmDia.Models;
using DoseEmDia.Models.db;
using DoseEmDia.Models.Enums;
using DoseEmDia.Models.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoseEmDia.Controllers
{
    public class VacinaService
    {
        private readonly ApplicationDbContext _context;

        public VacinaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Vacina>> ObterVacinasPorCpf(string cpf)
        {
            var usuario = await _context.Usuario
                .Include(u => u.Vacinas)
                .FirstOrDefaultAsync(u => u.CPF == cpf);

            if (usuario == null)
                throw UsuarioException.UsuarioNaoEncontradoPorCpf(cpf);

            if (usuario.Vacinas == null || !usuario.Vacinas.Any())
                throw VacinaException.NenhumaVacinaEncontrada(usuario.IdUser);

            return usuario.Vacinas
                .OrderByDescending(v =>
                    v.Status == StatusVacina.EmAtraso ? 1 :
                    v.Status == StatusVacina.AVencer ? 2 :
                    3)
                .ThenBy(v => v.DataAplicacao)
                .ToList();
        }
    }
}
