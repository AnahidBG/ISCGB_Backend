using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoGestionAPI.Models;
using Microsoft.AspNetCore.Authorization;
using AutoGestionAPI.DTOs;
using System.Security.Claims;

namespace AutoGestionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly TuDbContext _context;

        public UsuariosController(TuDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public IActionResult GetUsuarioById(int id)
        {
            var usuario = _context.Usuarios
                .Include(u => u.UsuariosRoles)
                    .ThenInclude(ur => ur.IdRolNavigation)
                .Where(u => u.IdUsuario == id)
                .Select(u => new
                {
                    IdUsuario = u.IdUsuario,
                    Dni = u.Dni,
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    Email = u.Email,
                    Telefono = u.Telefono,
                    TelefonoEmergencia = u.TelefonoEmergencia,
                    LugarNacimiento = u.LugarNacimiento,
                    ContactoEmergencia = u.ContactoEmergencia,
                    Direccion = u.Direccion,
                    IdProvincia = u.IdProvincia,
                    FechaNac = u.FechaNac,
                    EstadoUsuario = u.EstadoUsuario,
                    Roles = u.UsuariosRoles.Select(ur => new
                    {
                        IdRol = ur.IdRol,
                        NombreRol = ur.IdRolNavigation.Rol
                    }).ToList()
                })
                .FirstOrDefault();

            if (usuario == null)
                return NotFound(new { message = $"No se encontró el usuario con ID {id}." });

            return Ok(usuario);
        }

        // GET: api/Usuarios
        [HttpGet]
        public IActionResult GetUsuarios(
            [FromQuery] string? rol,
            [FromQuery] bool? estado,
            [FromQuery] int pagina = 1,
            [FromQuery] int registrosPorPagina = 10
        )
        {

            var query = _context.Usuarios
                .Include(u => u.UsuariosRoles)
                    .ThenInclude(ur => ur.IdRolNavigation)
                .AsQueryable();


            if (!string.IsNullOrEmpty(rol))
                query = query.Where(u => u.UsuariosRoles.Any(ur => ur.IdRolNavigation.Rol == rol));

            if (estado.HasValue)
                query = query.Where(u => u.EstadoUsuario == estado.Value);


            var totalRegistros = query.Count();


            var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)registrosPorPagina);


            var usuarios = query
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .Select(u => new
                {
                    IdUsuario = u.IdUsuario,
                    Dni = u.Dni,
                    NombreCompleto = u.Nombre + " " + u.Apellido,
                    Email = u.Email,
                    Telefono = u.Telefono,
                    EstadoUsuario = u.EstadoUsuario,
                    Roles = u.UsuariosRoles.Select(ur => new
                    {
                        IdRol = ur.IdRol,
                        NombreRol = ur.IdRolNavigation.Rol
                    }).ToList()
                }).ToList();

            if (usuarios.Count == 0)
                return NotFound(new { message = "No se encontraron usuarios con los criterios especificados." });


            return Ok(new
            {
                Paginacion = new
                {
                    TotalRegistros = totalRegistros,
                    TotalPaginas = totalPaginas,
                    PaginaActual = pagina,
                    RegistrosPorPagina = registrosPorPagina
                },
                Datos = usuarios
            });

        }
        [HttpPost("foto-perfil")]
        [Authorize] // Cualquier usuario logueado (Docente, Alumno, Director) puede subir su foto
        public async Task<IActionResult> SubirFotoPerfil([FromForm] SubirFotoDto dto)
        {
            // 1. Identificamos al usuario mediante su Token
            string? usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(usuarioIdClaim, out int idUsuario)) return Unauthorized();

            // 2. Validamos que realmente haya enviado un archivo
            if (dto.Foto == null || dto.Foto.Length == 0)
                return BadRequest(new { message = "No se envió ninguna imagen." });

            // 3. Validamos la extensión por seguridad
            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(dto.Foto.FileName).ToLower();
            if (!extensionesPermitidas.Contains(extension))
                return BadRequest(new { message = "Formato no válido. Solo se permiten JPG y PNG." });

            // 4. Preparamos la carpeta de destino física en el servidor (wwwroot/uploads/perfiles)
            string carpetaDestino = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "perfiles");
            if (!Directory.Exists(carpetaDestino))
            {
                Directory.CreateDirectory(carpetaDestino);
            }

            // 5. Generamos un nombre único para evitar que fotos con el mismo nombre se pisen
            string nombreArchivo = $"usuario_{idUsuario}_{Guid.NewGuid()}{extension}";
            string rutaFisica = Path.Combine(carpetaDestino, nombreArchivo);

            // 6. Guardamos el archivo físicamente en el disco
            using (var stream = new FileStream(rutaFisica, FileMode.Create))
            {
                await dto.Foto.CopyToAsync(stream);
            }

            // 7. Actualizamos la base de datos
            var usuario = await _context.Usuarios.FindAsync(idUsuario);
            if (usuario == null) return NotFound("Usuario no encontrado.");

            // (Opcional) Aquí podría agregar código para borrar la foto anterior del disco duro si ya tenía una

            // IMPORTANTE: Asegúrese de que la propiedad en su clase Usuario se llame así. 
            // Si la nombró diferente en migraciones pasadas, cámbielo aquí.
            usuario.FotoPerfil = $"/uploads/perfiles/{nombreArchivo}";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Foto de perfil actualizada exitosamente.",
                url = usuario.FotoPerfil
            });
        }

        [HttpPut("mi-perfil")]
        [Authorize]
        public async Task<IActionResult> ActualizarPerfil([FromBody] ActualizarPerfilDto dto)
        {
            string? usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(usuarioIdClaim, out int idUsuario)) return Unauthorized();

            var usuario = await _context.Usuarios.FindAsync(idUsuario);
            if (usuario == null) return NotFound(new { message = "Usuario no encontrado." });

            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != usuario.Email)
            {
                // Verificamos que el nuevo correo no esté siendo usado por OTRO usuario
                bool correoEnUso = await _context.Usuarios
                    .AnyAsync(u => u.Email == dto.Email && u.IdUsuario != idUsuario);

                if (correoEnUso)
                {
                    return BadRequest(new { message = "El correo electrónico ingresado ya está registrado a nombre de otra persona." });
                }

                usuario.Email = dto.Email;
            }

            // Mapeo del resto de campos editables
            if (!string.IsNullOrWhiteSpace(dto.Telefono))
                usuario.Telefono = dto.Telefono;

            if (!string.IsNullOrWhiteSpace(dto.TelefonoEmergencia))
                usuario.TelefonoEmergencia = dto.TelefonoEmergencia;

            if (!string.IsNullOrWhiteSpace(dto.LugarNacimiento))
                usuario.LugarNacimiento = dto.LugarNacimiento;

            if (!string.IsNullOrWhiteSpace(dto.ContactoEmergencia))
                usuario.ContactoEmergencia = dto.ContactoEmergencia;

            if (!string.IsNullOrWhiteSpace(dto.Direccion))
                usuario.Direccion = dto.Direccion;

            if (!string.IsNullOrWhiteSpace(dto.Genero))
                usuario.Genero = dto.Genero;

            if (!string.IsNullOrWhiteSpace(dto.AfiliacionEmergencia))
                usuario.AfiliacionEmergencia = dto.AfiliacionEmergencia;

            if (dto.IdProvincia.HasValue)
                usuario.IdProvincia = dto.IdProvincia.Value;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Datos del perfil actualizados correctamente." });
        }
    }
}