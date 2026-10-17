using Microsoft.AspNetCore.Mvc;
using AutoGestionAPI.Models;
using AutoGestionAPI.DTOs;
using System.IO;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AutoGestionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocenteController : ControllerBase
    {
        private readonly TuDbContext _context;

        public DocenteController(TuDbContext context)
        {
            _context = context;
        }
        [HttpPut("docentes/{idDocente}/suplencia")]
        [Authorize(Roles = "Director,Secretario")]
        public async Task<IActionResult> ConfigurarSuplencia(int idDocente, [FromBody] GestionarSuplenciaDto dto)
        {
            var docente = await _context.Docentes
                .Include(d => d.IdUsuarioNavigation)
                    .ThenInclude(u => u.UsuariosRoles)
                .FirstOrDefaultAsync(d => d.IdDocente == idDocente);

            if (docente == null)
                return NotFound(new { message = "No se encontró el registro del docente." });

            docente.EsSuplente = dto.EsSuplente;

            if (dto.EsSuplente)
            {
                docente.FechaInicioSuplencia = dto.FechaInicio ?? DateTime.Now;
                docente.FechaFinSuplencia = dto.FechaFin;


                if (!docente.IdUsuarioNavigation.EstadoUsuario)
                {
                    docente.IdUsuarioNavigation.EstadoUsuario = true;
                }


                if (!docente.IdUsuarioNavigation.UsuariosRoles.Any(r => r.IdRol == 3))
                {
                    _context.UsuariosRoles.Add(new UsuariosRole { IdUsuario = docente.IdUsuario, IdRol = 3 });
                }
            }

            else
            {

                if (docente.FechaFinSuplencia == null && docente.FechaInicioSuplencia != null)
                {
                    docente.FechaFinSuplencia = dto.FechaFin ?? DateTime.Now;
                }

                var usuario = docente.IdUsuarioNavigation;


                if (usuario.UsuariosRoles.Count == 1 && usuario.UsuariosRoles.Any(r => r.IdRol == 3))
                {
                    usuario.EstadoUsuario = false;
                }

                else if (usuario.UsuariosRoles.Count > 1)
                {
                    var rolDocente = usuario.UsuariosRoles.FirstOrDefault(r => r.IdRol == 3);
                    if (rolDocente != null)
                    {
                        _context.UsuariosRoles.Remove(rolDocente);
                    }
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = dto.EsSuplente ? "Suplencia y permisos activados." : "Baja procesada y accesos revocados correctamente.",
                esSuplente = docente.EsSuplente,
                estadoUsuario = docente.IdUsuarioNavigation.EstadoUsuario
            });
        }

        [HttpPut("responder-asignacion")]
        [Authorize(Roles = "Docente")]
        public async Task<IActionResult> ResponderAsignacion([FromBody] ResponderMesaDto dto)
        {
            // 1. Identificamos al docente que está respondiendo
            string? usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(usuarioIdClaim, out int idUsuario)) return Unauthorized();

            Docente? docente = await _context.Docentes.FirstOrDefaultAsync(d => d.IdUsuario == idUsuario);
            if (docente == null) return NotFound(new { message = "Docente no encontrado." });

            // 2. Buscamos exactamente la asignación pendiente de este docente para este examen
            MesaExamen? mesa = await _context.MesasExamenes
                .FirstOrDefaultAsync(m => m.IdExamen == dto.IdExamen && m.IdDocente == docente.IdDocente);

            if (mesa == null)
            {
                return NotFound(new { message = "No se encontró la asignación a esta mesa de examen." });
            }

            if (mesa.EstadoConfirmacion != "Pendiente")
            {
                return BadRequest(new { message = $"Esta asignación ya fue respondida anteriormente ({mesa.EstadoConfirmacion})." });
            }

            // 3. Actualizamos el estado basándonos en lo que envió el frontend
            mesa.EstadoConfirmacion = dto.Acepta ? "Aceptada" : "Rechazada";

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Ha {mesa.EstadoConfirmacion.ToLower()} la asignación a la mesa de examen." });
        }
    }
}
