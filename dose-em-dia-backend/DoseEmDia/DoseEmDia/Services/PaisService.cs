using DoseEmDia.Models.db;
using DoseEmDia.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoseEmDia.Services
{
    public class PaisService
    {
        private readonly ApplicationDbContext _context;

        public PaisService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task PopularPaisesSeNecessarioAsync()
        {
            if (_context.Paises.Any()) return;

            var paises = new List<Pais>
            {
                new Pais { Nome = "Estados Unidos", Url = "https://www.cdc.gov/vaccines/index.html" },
                new Pais { Nome = "Argentina", Url = "https://www.argentina.gob.ar/salud/mosquitos/viajeros" },
                new Pais { Nome = "França", Url = "https://sante.gouv.fr/prevention-en-sante/sante-des-populations/article/recommandations-sanitaires-pour-les-voyageurs" },
                new Pais { Nome = "Itália", Url = "https://www.epicentro.iss.it/viaggiatori/" },
                new Pais { Nome = "Espanha", Url = "https://www.sanidad.gob.es/areas/sanidadExterior/laSaludTambienViaja/consejosSanitarios/infVacunas.htm" },
                new Pais { Nome = "Reino Unido", Url = "https://www.nhs.uk/vaccinations/travel-vaccinations/travel-vaccination-advice/" },
                new Pais { Nome = "Chile", Url = "https://saludresponde.minsal.cl/recomendaciones-de-salud-en-caso-de-viaje/" },
                new Pais { Nome = "Alemanha", Url = "https://www.bundesgesundheitsministerium.de/themen/praevention/impfungen.html" },
                new Pais { Nome = "Portugal", Url = "https://www.sns24.gov.pt/servico/consulta-do-viajante/" },
                new Pais { Nome = "Paraguai", Url = "https://dgvs.mspbs.gov.py/salud-del-viajero/requisitos-para-ingresar-a-paraguay/" },
                new Pais { Nome = "Uruguai", Url = "https://www.gub.uy/ministerio-salud-publica/tematica/salud-viajeros" },
                new Pais { Nome = "México", Url = "https://www.gob.mx/salud/acciones-y-programas/vacunacion-para-viajeros-internacionales" },
                new Pais { Nome = "Canadá", Url = "https://travel.gc.ca/travelling/health-safety/vaccines" },
                new Pais { Nome = "Peru", Url = "https://www.peru.travel/es/datos-utiles/salud" },
                new Pais { Nome = "Japão", Url = "https://www.mofa.go.jp/ca/cp/page22e_000925.html" }
            };

            _context.Paises.AddRange(paises);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Pais>> ListarPaisesVacinasObrigatorias()
        {
            return await _context.Paises
                .OrderBy(p => p.Nome)
                .ToListAsync();
        }
    }
}
