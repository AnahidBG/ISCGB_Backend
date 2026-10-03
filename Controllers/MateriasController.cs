using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoGestionAPI.Models;
using AutoGestionAPI.DTOs;
using AutoGestionAPI.Services;

namespace AutoGestionAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AsignacionesController : ControllerBase
    {

        private readonly TuDbContext _context;


        public AsignacionesController(TuDbContext context)
        {
            _context = context;
        }

        // --- 1. ENDPOINT PARA LLENAR EL SELECT DE MATERIAS EN EL FRONTEND ---
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

        // --- 2. ENDPOINT PARA LLENAR EL SELECT DE DOCENTES EN EL FRONTEND ---
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

        // --- 3. ENDPOINT PARA GUARDAR LA ASIGNACIÓN (LA CARGA REAL) ---
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

            // 2. Creamos el objeto usando el nombre correcto de la clase (DocenteMateria) e incluimos la comisión
            var nuevaAsignacion = new DocenteMaterium
            {
                IdDocente = dto.IdDocente,
                IdMateria = dto.IdMateria,
                IdComision = dto.IdComision
            };

            // 3. Guardamos en la base de datos
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