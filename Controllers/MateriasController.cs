using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoGestionAPI.Models;

namespace AutoGestionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MateriasController : ControllerBase
    {
        private readonly TuDbContext _context;

        public MateriasController(TuDbContext context)
        {
            _context = context;
        }

        [HttpGet("mis-materias")]
        [Authorize(Roles = "Alumno")]
        public async Task<IActionResult> ObtenerMisMaterias()
        {
            string? usuarioIdClaim = User.Claims
                .FirstOrDefault(c => c.Type == "id")?.Value;

            if (!int.TryParse(usuarioIdClaim, out int idUsuario))
            {
                return Unauthorized("Token inválido.");
            }

            var alumno = await _context.Alumnos
                .FirstOrDefaultAsync(a => a.IdUsuario == idUsuario);

            if (alumno == null)
            {
                return NotFound("El usuario no tiene un perfil de alumno.");
            }

            var materias = await (
                from am in _context.AlumnoMateria
                join m in _context.Materias
                    on am.IdMateria equals m.IdMateria
                where am.IdAlumno == alumno.IdAlumno
                select new
                {
                    IdMateria = m.IdMateria,
                    Nombre = m.Nombre,
                    Carrera = m.Carrera,
                    Curso = m.Curso
                }
            )
            .Distinct()
            .ToListAsync();

            return Ok(materias);
        }
    }
}