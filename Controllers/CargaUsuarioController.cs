using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoGestionAPI.Models; // Ajustá al namespace de tus modelos
using AutoGestionAPI.DTOs.Usuarios; // Ajustá al namespace de tu DTO

namespace AutoGestionAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Bloqueo de seguridad: Solo entran Directores y Secretarios
    //[Authorize(Roles = "Director,Secretario")] 
    public class UsuariosAdminController : ControllerBase
    {
        private readonly TuDbContext _context; // Cambiá por el nombre real de tu DbContext (ej: IscgbContext)

        public UsuariosAdminController(TuDbContext context)
        {
            _context = context;
        }

        [HttpPost("alta")]
        public async Task<IActionResult> AltaUsuario([FromBody] CargaUsuarioDto dto)
        {
            
            if (dto.IdRol < 1 || dto.IdRol > 4)
                return BadRequest("Rol inválido. Solo se permite asignar Director, Secretario, Docente o Alumno.");

            
            if (dto.IdRol == 3 && dto.EsDirectorSuplente)
            {
                var existeSuplente = await _context.Docentes
                    .Include(d => d.IdUsuarioNavigation)
                    .FirstOrDefaultAsync(d => d.DirectorSuplente == true && d.IdUsuarioNavigation.EstadoUsuario == true);
                
                if (existeSuplente != null)
                {
                    return BadRequest($"Ya existe un director suplente asignado con el nombre: {existeSuplente.IdUsuarioNavigation.Nombre} {existeSuplente.IdUsuarioNavigation.Apellido}.");
                }
            }

            
            var nuevoUsuario = new Usuario
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Dni = dto.Dni,
                Cuil = dto.Cuil,
                Email = dto.Email,
                Genero = dto.Genero,
                Direccion = dto.Direccion,
                Telefono = dto.Telefono,
                ContactoEmergencia = dto.ContactoEmergencia,
                TelefonoEmergencia = dto.TelefonoEmergencia,
                AfiliacionEmergencia = dto.AfiliacionEmergencia,
                IdProvincia = dto.IdProvincia,
                FechaNac = dto.FechaNac,
                EstadoUsuario = true, 
                PasswordHash = "AsignarContraseñaTemporal" 
            };

            
            nuevoUsuario.UsuariosRoles.Add(new UsuariosRole { IdRol = dto.IdRol });

            
            if (dto.IdRol == 3) 
            {
                nuevoUsuario.Docentes.Add(new Docente
                {
                    DirectorSuplente = dto.EsDirectorSuplente
                });
            }
            else if (dto.IdRol == 4) 
            {
                nuevoUsuario.Alumnos.Add(new Alumno()); 
            }

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return Ok(new 
            { 
                mensaje = "Usuario creado exitosamente.", 
                legajoAutocompletado = nuevoUsuario.Dni 
            });
        }

        [HttpPut("modificar/{id}")]
        public async Task<IActionResult> ModificarUsuario(int id, [FromBody] CargaUsuarioDto dto)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Docentes)
                .Include(u => u.UsuariosRoles)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null) return NotFound("Usuario no encontrado.");

            
            var docente = usuario.Docentes.FirstOrDefault();
            if (docente != null && dto.EsDirectorSuplente && docente.DirectorSuplente != true)
            {
                var existeSuplente = await _context.Docentes
                    .Include(d => d.IdUsuarioNavigation)
                    .FirstOrDefaultAsync(d => d.DirectorSuplente == true && d.IdUsuarioNavigation.EstadoUsuario == true);
                
                if (existeSuplente != null)
                    return BadRequest($"Ya existe un director suplente asignado con el nombre: {existeSuplente.IdUsuarioNavigation.Nombre} {existeSuplente.IdUsuarioNavigation.Apellido}.");
                
                docente.DirectorSuplente = true;
            }
            else if (docente != null && !dto.EsDirectorSuplente)
            {
                docente.DirectorSuplente = false;
            }

            
            usuario.Nombre = dto.Nombre;
            usuario.Apellido = dto.Apellido;
            

            await _context.SaveChangesAsync();

            
            return Ok(new { message = "El perfil del usuario ha sido actualizado correctamente" });
        }

        [HttpPut("baja/{id}")]
        public async Task<IActionResult> BajaUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Docentes)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null) return NotFound("Usuario no encontrado.");

            
            usuario.EstadoUsuario = false; 
            
           
            var docente = usuario.Docentes.FirstOrDefault();
            if (docente != null && docente.DirectorSuplente == true)
            {
                docente.DirectorSuplente = false;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "El usuario ha sido dado de baja (inactivo) correctamente." });
        }
    }
}