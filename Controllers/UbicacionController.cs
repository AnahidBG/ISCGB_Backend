using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoGestionAPI.Models;
using AutoGestionAPI.DTOs;

namespace TuProyecto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UbicacionesController : ControllerBase
    {
        private readonly TuDbContext _context;

        public UbicacionesController(TuDbContext context)
        {
            _context = context;
        }

        // Endpoint 1: El front lo llama para llenar el primer desplegable (Países)
        [HttpGet("paises")]
        public async Task<IActionResult> ObtenerPaises()
        {
            var paises = await _context.Pais
                .Select(p => new
                {
                    IdPais = p.IdPais,
                    Nombre = p.Nombre
                })
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return Ok(paises);
        }

        // Endpoint 2: Cuando el usuario elige un país, el front llama a este endpoint 
        // pasándole el ID del país elegido para llenar el segundo desplegable (Provincias)
        [HttpGet("paises/{idPais}/provincias")]
        public async Task<IActionResult> ObtenerProvincias(int idPais)
        {
            var provincias = await _context.Provincia
                .Where(p => p.IdPais == idPais)
                .Select(p => new
                {
                    IdProvincia = p.IdProvincia,
                    Nombre = p.Nombre
                })
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            if (!provincias.Any())
                return NotFound(new { message = "No se encontraron provincias para este país." });

            return Ok(provincias);
        }
    }
}