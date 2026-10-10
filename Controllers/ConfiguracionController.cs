using Microsoft.AspNetCore.Mvc;
using AutoGestionAPI.Models;
using AutoGestionAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AutoGestionAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfiguracionController : ControllerBase
    {
        private readonly TuDbContext _context;

        public ConfiguracionController(TuDbContext context)
        {
            _context = context;
        }

        [HttpGet("frecuencia-notificaciones")]
        public async Task<IActionResult> ObtenerFrecuencia()
        {
            var config = await _context.ConfiguracionesSistema.FirstOrDefaultAsync();

            int dias = config?.FrecuenciaNotificacionDias ?? 7;

            return Ok(new { diasFrecuencia = dias });
        }

        [HttpPut("frecuencia-notificaciones")]
        public async Task<IActionResult> ActualizarFrecuencia([FromBody] ConfiguracionNotificacionDto dto)
        {
            if (dto.DiasFrecuencia <= 0)
                return BadRequest(new { message = "La frecuencia debe ser mayor a 0 días." });

            var config = await _context.ConfiguracionesSistema.FirstOrDefaultAsync();

            if (config == null)
            {
                config = new ConfiguracionSistema { FrecuenciaNotificacionDias = dto.DiasFrecuencia };
                _context.ConfiguracionesSistema.Add(config);
            }
            else
            {
                config.FrecuenciaNotificacionDias = dto.DiasFrecuencia;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Frecuencia actualizada a {dto.DiasFrecuencia} días exitosamente." });
        }

        [HttpGet("mis-notificaciones")]
        [Authorize(Roles = "Docente")]
        public async Task<IActionResult> ObtenerNotificacionesPendientes()
        {
            // 1. Identificamos al docente por su token
            string? usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(usuarioIdClaim, out int idUsuario)) return Unauthorized();

            var docente = await _context.Docentes.FirstOrDefaultAsync(d => d.IdUsuario == idUsuario);
            if (docente == null) return NotFound(new { message = "Docente no encontrado." });

            // 2. Buscamos las mesas donde fue asignado y el estado sigue siendo "Pendiente"
            var notificaciones = await _context.MesasExamenes
                .Include(m => m.IdExamenNavigation)
                    .ThenInclude(e => e.IdMateriaNavigation) // Para mostrarle el nombre de la materia
                .Where(m => m.IdDocente == docente.IdDocente && m.EstadoConfirmacion == "Pendiente")
                .Select(m => new
                {
                    IdExamen = m.IdExamen,
                    Materia = m.IdExamenNavigation.IdMateriaNavigation.Nombre,
                    Fecha = m.IdExamenNavigation.Fecha,
                    Mensaje = $"Ha sido asignado a la mesa de {m.IdExamenNavigation.IdMateriaNavigation.Nombre} el día {m.IdExamenNavigation.Fecha:dd/MM/yyyy HH:mm}"
                })
                .ToListAsync();

            return Ok(notificaciones);
        }
    }
}