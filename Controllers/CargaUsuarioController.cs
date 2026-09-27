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
            // Validación de roles permitidos
            if (dto.IdRol < 1 || dto.IdRol > 4)
                return BadRequest("Rol inválido. Solo se permite asignar Director, Secretario, Docente o Alumno.");

            // Validación de Director Suplente (Regla de negocio)
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

            // Armamos el Usuario con los campos de tu base de datos
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
                EstadoUsuario = true, // Estado activo por defecto (Alta)
                PasswordHash = "AsignarContraseñaTemporal" // Falta encriptar un hash real
            };

            // Agregamos el Rol a la tabla intermedia
            nuevoUsuario.UsuariosRoles.Add(new UsuariosRole { IdRol = dto.IdRol });

            // Inserción en tablas específicas según el rol
            if (dto.IdRol == 3) // Docente
            {
                nuevoUsuario.Docentes.Add(new Docente
                {
                    DirectorSuplente = dto.EsDirectorSuplente
                });
            }
            else if (dto.IdRol == 4) // Alumno
            {
                nuevoUsuario.Alumnos.Add(new Alumno()); // Si la tabla alumno tiene campos obligatorios, agregalos acá
            }

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            // Cumplimos con el Criterio: "Que se autocomplete legajo con DNI"
            // Retornamos el DNI como el número de legajo autogenerado para que el frontend lo muestre
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

            // Si es docente y le están asignando la suplencia, validamos que no haya otro
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

            // Actualización de datos personales
            usuario.Nombre = dto.Nombre;
            usuario.Apellido = dto.Apellido;
            // ... (podés mapear el resto de los campos de la misma forma)

            await _context.SaveChangesAsync();

            // Criterio exacto de la tarjeta: Mensaje de configuración guardada
            return Ok(new { message = "El perfil del usuario ha sido actualizado correctamente" });
        }

        [HttpPut("baja/{id}")]
        public async Task<IActionResult> BajaUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Docentes)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null) return NotFound("Usuario no encontrado.");

            // Criterio: Cambio a inactivo sin borrar documentación
            usuario.EstadoUsuario = false; 
            
            // Si era director suplente, le liberamos el cargo para que otro pueda asumirlo
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