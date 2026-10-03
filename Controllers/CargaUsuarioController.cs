using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoGestionAPI.Models;
using AutoGestionAPI.DTOs.Usuarios;
using AutoGestionAPI.Services;

namespace AutoGestionAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // Bloqueo de seguridad: Solo entran Directores y Secretarios
    //[Authorize(Roles = "Director,Secretario")] 
    public class UsuariosAdminController : ControllerBase
    {
        private readonly TuDbContext _context; // Cambiá por el nombre real de tu DbContext (ej: IscgbContext)
        private readonly IEmailService _emailService;

        public UsuariosAdminController(TuDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpPost("alta")]
        public async Task<IActionResult> AltaUsuario([FromBody] CargaUsuarioDto dto)
        {
            if (dto.IdRol < 1 || dto.IdRol > 4)
                return BadRequest(new { message = "Rol inválido. Solo se permite asignar Director, Secretario, Docente o Alumno." });

            bool dniExiste = await _context.Usuarios.AnyAsync(u => u.Dni == dto.Dni);

            if (dniExiste)
            {
                return BadRequest(new { message = $"Ya existe un usuario registrado con el DNI {dto.Dni}." });
            }

            bool emailExiste = await _context.Usuarios.AnyAsync(u => u.Email == dto.Email);
            if (emailExiste)
            {
                return BadRequest(new { message = $"El correo electrónico {dto.Email} ya está en uso." });
            }
            if (dto.IdRol == 3 && dto.EsDirectorSuplente)
            {
                var existeSuplente = await _context.Docentes
                    .Include(d => d.IdUsuarioNavigation)
                    .FirstOrDefaultAsync(d => d.DirectorSuplente == true && d.IdUsuarioNavigation.EstadoUsuario == true);

                if (existeSuplente != null)
                {
                    return BadRequest(new { message = $"Ya existe un director suplente asignado con el nombre: {existeSuplente.IdUsuarioNavigation.Nombre} {existeSuplente.IdUsuarioNavigation.Apellido}." });
                }
            }

            string tokenConfiguracion = Guid.NewGuid().ToString("N");

            var nuevoUsuario = new Usuario
            {
                Nombre = dto.Nombre,
                Apellido = dto.Apellido,
                Dni = dto.Dni,
                Email = dto.Email,
                Cuil = dto.Cuil,
                Genero = dto.Genero,
                Direccion = dto.Direccion,
                Telefono = dto.Telefono,
                IdProvincia = dto.IdProvincia,
                FechaNac = dto.FechaNac,
                ContactoEmergencia = dto.ContactoEmergencia,
                TelefonoEmergencia = dto.TelefonoEmergencia,
                AfiliacionEmergencia = dto.AfiliacionEmergencia,
                EstadoUsuario = true,


                PasswordHash = "PENDIENTE_CONFIGURACION",


                TokenRecuperacion = tokenConfiguracion,
                ExpiracionToken = DateTime.UtcNow.AddDays(20)
            };

            // Orden para asignar el rol 
            // Director = 1 
            // Secretario = 2
            // Docente = 3
            // Alumno = 4
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
                nuevoUsuario.Alumnos.Add(new Alumno
                {
                    Legajo = dto.Dni
                });
            }

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            // 2. Enviar el enlace por correo
            try
            {
                // El frontend debe tener una ruta que reciba este token, ej: /crear-password?token=abc123def456
                string urlConfiguracion = $"https://tu-frontend.com/crear-password?token={tokenConfiguracion}";
                await _emailService.EnviarLinkConfiguracionAsync(dto.Email, dto.Nombre, urlConfiguracion);
            }
            catch (Exception ex)
            {
                return Ok(new { mensaje = "Usuario creado, pero hubo un error al enviar el correo.", error = ex.Message });
            }

            return Ok(new { mensaje = "Usuario creado. Se envió un correo para configurar la contraseña." });
        }

        public class EstablecerPasswordDto
        {
            public string Token { get; set; } = string.Empty;
            public string NuevaPassword { get; set; } = string.Empty;
        }

        [HttpPost("establecer-password")]
        public async Task<IActionResult> EstablecerPassword([FromBody] EstablecerPasswordDto dto)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.TokenRecuperacion == dto.Token);

            if (usuario == null)
                return BadRequest("El enlace es inválido.");

            if (usuario.ExpiracionToken < DateTime.UtcNow)
                return BadRequest("El enlace ha expirado. Solicite uno nuevo.");


            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NuevaPassword);


            usuario.TokenRecuperacion = null;
            usuario.ExpiracionToken = null;

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Contraseña configurada exitosamente. Ya puede iniciar sesión." });
        }


        [HttpPut("modificar/{id}")]
        public async Task<IActionResult> ModificarUsuario(int id, [FromBody] CargaUsuarioDto dto)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Docentes)
                .Include(u => u.UsuariosRoles)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuario == null) return NotFound(new { message = "Usuario no encontrado." });

            // 1. Validar y actualizar DNI solo si fue enviado
            if (!string.IsNullOrWhiteSpace(dto.Dni))
            {
                bool dniExiste = await _context.Usuarios.AnyAsync(u => u.Dni == dto.Dni && u.IdUsuario != id);
                if (dniExiste)
                    return BadRequest(new { message = $"Ya existe otro usuario registrado con el DNI {dto.Dni}." });

                usuario.Dni = dto.Dni;
            }

            // 2. Validar y actualizar Email solo si fue enviado
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                bool emailExiste = await _context.Usuarios.AnyAsync(u => u.Email == dto.Email && u.IdUsuario != id);
                if (emailExiste)
                    return BadRequest(new { message = $"El correo electrónico {dto.Email} ya está en uso por otra persona." });

                usuario.Email = dto.Email;
            }

            // 3. Lógica del Director Suplente
            var docente = usuario.Docentes.FirstOrDefault();
            if (docente != null)
            {
                // Solo verificamos suplencia si nos mandan el dato explícitamente. 
                if (dto.EsDirectorSuplente && docente.DirectorSuplente != true)
                {
                    var existeSuplente = await _context.Docentes
                        .Include(d => d.IdUsuarioNavigation)
                        .FirstOrDefaultAsync(d => d.DirectorSuplente == true && d.IdUsuarioNavigation.EstadoUsuario == true);

                    if (existeSuplente != null)
                        return BadRequest(new { message = $"Ya existe un director suplente asignado con el nombre: {existeSuplente.IdUsuarioNavigation.Nombre} {existeSuplente.IdUsuarioNavigation.Apellido}." });

                    docente.DirectorSuplente = true;
                }
                else if (!dto.EsDirectorSuplente)
                {
                    docente.DirectorSuplente = false;
                }
            }

            // 4. Actualizar el resto de los campos SOLO si no vienen vacíos o nulos
            if (!string.IsNullOrWhiteSpace(dto.Nombre)) usuario.Nombre = dto.Nombre;
            if (!string.IsNullOrWhiteSpace(dto.Apellido)) usuario.Apellido = dto.Apellido;
            if (!string.IsNullOrWhiteSpace(dto.Cuil)) usuario.Cuil = dto.Cuil;
            if (!string.IsNullOrWhiteSpace(dto.Genero)) usuario.Genero = dto.Genero;
            if (!string.IsNullOrWhiteSpace(dto.Direccion)) usuario.Direccion = dto.Direccion;
            if (!string.IsNullOrWhiteSpace(dto.Telefono)) usuario.Telefono = dto.Telefono;
            if (!string.IsNullOrWhiteSpace(dto.ContactoEmergencia)) usuario.ContactoEmergencia = dto.ContactoEmergencia;
            if (!string.IsNullOrWhiteSpace(dto.TelefonoEmergencia)) usuario.TelefonoEmergencia = dto.TelefonoEmergencia;
            if (!string.IsNullOrWhiteSpace(dto.AfiliacionEmergencia)) usuario.AfiliacionEmergencia = dto.AfiliacionEmergencia;

            if (dto.IdProvincia != null && dto.IdProvincia > 0) usuario.IdProvincia = dto.IdProvincia;
            if (dto.FechaNac != null) usuario.FechaNac = dto.FechaNac;

            if (dto.IdRol > 0 && dto.IdRol <= 4)
            {
                var rolActual = usuario.UsuariosRoles.FirstOrDefault();

                if (rolActual == null || rolActual.IdRol != dto.IdRol)
                {
                    if (usuario.UsuariosRoles.Any())
                    {
                        _context.UsuariosRoles.RemoveRange(usuario.UsuariosRoles);
                    }

                    var nuevoRol = new UsuariosRole
                    {
                        IdUsuario = usuario.IdUsuario,
                        IdRol = dto.IdRol.Value
                    };

                    _context.UsuariosRoles.Add(nuevoRol);
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "El perfil del usuario ha sido actualizado correctamente." });
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

        [HttpPut("alta/{id}")]
        public async Task<IActionResult> AltaUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null) return NotFound(new { message = "Usuario no encontrado." });


            if (usuario.EstadoUsuario == true)
                return BadRequest(new { message = "El usuario ya se encuentra activo en el sistema." });

            usuario.EstadoUsuario = true;

            await _context.SaveChangesAsync();

            return Ok(new { message = "El usuario ha sido reactivado (activo) correctamente." });
        }
    }
}