using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoGestionAPI.Models;
using AutoGestionAPI.DTOs;
using AutoGestionAPI.Services;
using System.Security.Claims;

namespace AutoGestionAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            string? usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(usuarioIdClaim, out int idUsuario))
            {
                return Unauthorized("Token inválido.");
            }

            var alumno = await _context.Alumnos.FirstOrDefaultAsync(a => a.IdUsuario == idUsuario);

            if (alumno == null)
            {
                return NotFound("El usuario no tiene un perfil de alumno.");
            }

            var materias = await (
                from am in _context.AlumnoMateria
                join m in _context.Materias on am.IdMateria equals m.IdMateria
                where am.IdAlumno == alumno.IdAlumno
                select new
                {
                    IdMateria = m.IdMateria,
                    Nombre = m.Nombre,
                    Carrera = m.Carrera,
                    Curso = m.Curso
                }
            ).Distinct().ToListAsync();

            return Ok(materias);
        }


        [HttpGet("materias-disponibles")]
        public async Task<IActionResult> ObtenerMaterias()
        {
            var materias = await _context.Materias
                .Select(m => new
                {
                    IdMateria = m.IdMateria,
                    NombreMateria = m.Nombre
                })
                .OrderBy(m => m.NombreMateria)
                .ToListAsync();

            return Ok(new { data = materias });
        }

        [HttpGet("docentes-disponibles")]
        public async Task<IActionResult> ObtenerDocentes()
        {
            var docentes = await _context.Docentes
                .Include(d => d.IdUsuarioNavigation)
                .Select(d => new
                {
                    IdDocente = d.IdDocente,
                    NombreCompleto = d.IdUsuarioNavigation.Nombre + " " + d.IdUsuarioNavigation.Apellido
                })
                .OrderBy(d => d.NombreCompleto)
                .ToListAsync();

            return Ok(new { data = docentes });
        }

        [HttpGet("comisiones-disponibles")]
        public async Task<IActionResult> ObtenerComisiones()
        {
            var comisiones = await _context.Comisions
                .Select(c => new
                {
                    IdComision = c.IdComision,
                    NombreComision = c.Comision1
                })
                .OrderBy(c => c.NombreComision)
                .ToListAsync();

            return Ok(new { data = comisiones });
        }

        [HttpPost("asignar")]
        public async Task<IActionResult> AsignarMateriaADocente([FromBody] AsignarMateriaDto dto)
        {
            var asignacionExiste = await _context.DocenteMateria
                .AnyAsync(dm => dm.IdDocente == dto.IdDocente &&
                                dm.IdMateria == dto.IdMateria &&
                                dm.IdComision == dto.IdComision);

            if (asignacionExiste)
            {
                return BadRequest(new { message = "El docente ya tiene asignada esta materia en esta comisión." });
            }

            var nuevaAsignacion = new DocenteMaterium
            {
                IdDocente = dto.IdDocente,
                IdMateria = dto.IdMateria,
                IdComision = dto.IdComision
            };

            _context.DocenteMateria.Add(nuevaAsignacion);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Materia asignada correctamente al docente en la comisión indicada." });
        }

        [HttpPost("cargar-materia")]
        public async Task<IActionResult> AgregarMateria([FromBody] CargarMateriaDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.NombreMateria))
            {
                return BadRequest(new { message = "El nombre de la materia es obligatorio." });
            }

            var materiaExiste = await _context.Materias
                .AnyAsync(m => m.Nombre.ToLower() == dto.NombreMateria.ToLower());

            if (materiaExiste)
            {
                return BadRequest(new { message = "Ya existe una materia con ese nombre en el sistema." });
            }

            Materia nuevaMateria = new Materia
            {
                Nombre = dto.NombreMateria,
                Carrera = dto.Carrera,
                Curso = dto.Curso
            };

            _context.Materias.Add(nuevaMateria);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Materia creada correctamente.",
                idMateria = nuevaMateria.IdMateria
            });
        }


    }
}