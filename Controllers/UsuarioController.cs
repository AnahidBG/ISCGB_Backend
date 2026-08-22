using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoGestionAPI.Models;

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
                    FechaNac = u.fecha_nac,
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
    }
}