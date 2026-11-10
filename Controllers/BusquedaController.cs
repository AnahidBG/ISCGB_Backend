using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoGestionAPI.Models;
using AutoGestionAPI.DTOs;
using AutoGestionAPI.Seguridad;
using Microsoft.AspNetCore.Authorization;

namespace AutoGestionAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Devuelve nombres y DNIs de todo el instituto: solo Dirección/Secretaría.
    [Authorize(Roles = UsuarioActual.RolesAdministrativos)]
    public class BuscadorController : ControllerBase
    {
        private readonly TuDbContext _context;

        public BuscadorController(TuDbContext context)
        {
            _context = context;
        }

        [HttpGet("global")]
        public async Task<IActionResult> BuscarGlobal([FromQuery] string termino)
        {
            // 1. Evitar búsquedas vacías o muy cortas por rendimiento
            if (string.IsNullOrWhiteSpace(termino) || termino.Length < 2)
            {
                return Ok(new { data = new List<ResultadoBusquedaDto>() });
            }

            string terminoBusqueda = termino.ToLower();
            List<ResultadoBusquedaDto> resultados = new List<ResultadoBusquedaDto>();

            List<ResultadoBusquedaDto> docentesEncontrados = await _context.Usuarios

                .Where(u => u.Nombre.ToLower().Contains(terminoBusqueda) ||
                            u.Apellido.ToLower().Contains(terminoBusqueda))
                .Select(u => new ResultadoBusquedaDto
                {
                    Tipo = "Docente",
                    Titulo = u.Nombre + " " + u.Apellido,
                    Subtitulo = "Legajo/DNI: " + u.Dni,
                    IdReferencia = u.IdUsuario
                })
                .ToListAsync();

            resultados.AddRange(docentesEncontrados);

            List<ResultadoBusquedaDto> materiasEncontradas = await _context.Materias
                .Where(m => m.Nombre.ToLower().Contains(terminoBusqueda))
                .Select(m => new ResultadoBusquedaDto
                {
                    Tipo = "Materia",
                    Titulo = m.Nombre,
                    Subtitulo = "Materia del sistema",
                    IdReferencia = m.IdMateria
                })
                .ToListAsync();

            resultados.AddRange(materiasEncontradas);

            List<ResultadoBusquedaDto> documentosEncontrados = await _context.Justificativos
                .Where(j => j.TipoInasistencia.ToLower().Contains(terminoBusqueda) ||
                            (j.NotaAdicional != null && j.NotaAdicional.ToLower().Contains(terminoBusqueda)))
                .Select(j => new ResultadoBusquedaDto
                {
                    Tipo = "Documento",
                    Titulo = j.TipoInasistencia,
                    Subtitulo = "Estado: " + j.Estado,
                    IdReferencia = j.IdJustificativo
                })
                .ToListAsync();

            resultados.AddRange(documentosEncontrados);

            return Ok(new
            {
                cantidad = resultados.Count,
                data = resultados.OrderBy(r => r.Titulo).ToList()
            });
        }
    }
}