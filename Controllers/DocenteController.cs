using Microsoft.AspNetCore.Mvc;
using AutoGestionAPI.Models;
using AutoGestionAPI.DTOs;
using System.IO;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
    }
}
