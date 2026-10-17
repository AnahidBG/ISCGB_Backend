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
        [Authorize(Roles = "Director,Secretario")]
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
        [HttpGet("alertas-rechazos")]
        [Authorize(Roles = "Director,Secretario")] // Restringido estrictamente a los mandos administrativos
        public async Task<IActionResult> ObtenerAsignacionesRechazadas()
        {
            // Buscamos todas las asignaciones que los docentes hayan rechazado
            var alertas = await _context.MesasExamenes
                .Include(m => m.IdDocenteNavigation)
                    .ThenInclude(d => d.IdUsuarioNavigation) // Navegamos para obtener el nombre del profesor
                .Include(m => m.IdExamenNavigation)
                    .ThenInclude(e => e.IdMateriaNavigation) // Navegamos para obtener la materia
                .Where(m => m.EstadoConfirmacion == "Rechazada")
                .Select(m => new
                {
                    IdExamen = m.IdExamen,
                    IdDocente = m.IdDocente,
                    Docente = m.IdDocenteNavigation.IdUsuarioNavigation.Nombre + " " + m.IdDocenteNavigation.IdUsuarioNavigation.Apellido,
                    Materia = m.IdExamenNavigation.IdMateriaNavigation.Nombre,
                    Fecha = m.IdExamenNavigation.Fecha,
                    Mensaje = "Asignación rechazada. Requiere reasignar a un nuevo docente de forma urgente."
                })
                .OrderBy(m => m.Fecha) // Ordenamos para que vean primero las mesas más próximas en el calendario
                .ToListAsync();

            return Ok(alertas);
        }
        [HttpPut("ocultar-alerta-rechazo")]
        [Authorize(Roles = "Director,Secretario")]
        public async Task<IActionResult> OcultarAlertaRechazo([FromBody] OcultarAlertaDto dto)
        {
            // 1. Buscamos el registro exacto de ese profesor y ese examen
            var mesa = await _context.MesasExamenes
                .FirstOrDefaultAsync(m => m.IdExamen == dto.IdExamen && m.IdDocente == dto.IdDocente);

            if (mesa == null)
            {
                return NotFound(new { message = "No se encontró el registro de la asignación." });
            }

            // 2. Verificamos que realmente esté en estado "Rechazada"
            if (mesa.EstadoConfirmacion == "Rechazada")
            {
                // 3. Cambiamos el estado para archivar la alerta sin borrar el historial En caso de necesitar un reporte de docentes que rechazaron mesas de exámenes se puede buscar en la base de datos como "Rechazada_Vista".
                mesa.EstadoConfirmacion = "Rechazada_Vista";

                await _context.SaveChangesAsync();

                return Ok(new { message = "Alerta archivada. El rechazo ha quedado guardado en el historial." });
            }

            return BadRequest(new { message = "Esta alerta ya fue procesada o no se encuentra en estado de rechazo." });
        }
    }
}