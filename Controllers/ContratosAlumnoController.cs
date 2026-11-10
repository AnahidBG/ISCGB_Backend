using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoGestionAPI.DTOs.Contratos;
using AutoGestionAPI.Models;
using AutoGestionAPI.Seguridad;

namespace AutoGestionAPI.Controllers
{
    /// <summary>
    /// Contratos firmados de los alumnos (por ejemplo, el convenio de la beca),
    /// SCRUM-227. Dirección o Secretaría los cargan y el alumno los ve desde su panel.
    ///
    /// Los PDF se guardan FUERA de wwwroot (carpeta "Almacenamiento/contratos"):
    /// son documentos personales y solo se entregan por GET {id}/pdf, que controla
    /// quién los pide.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContratosAlumnoController : ControllerBase
    {
        private const int LargoMaximoNombre = 100;

        private readonly TuDbContext _context;
        private readonly string _carpetaContratos;

        public ContratosAlumnoController(TuDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _carpetaContratos = Path.Combine(env.ContentRootPath, "Almacenamiento", "contratos");
        }

        // POST: api/ContratosAlumno — carga un contrato en el perfil de un alumno
        [HttpPost]
        [Authorize(Roles = UsuarioActual.RolesAdministrativos)]
        public async Task<IActionResult> Cargar([FromForm] CargarContratoAlumnoDto dto)
        {
            string nombre = dto.Nombre?.Trim() ?? "";
            if (nombre.Length == 0)
                return BadRequest(new { message = "El nombre del contrato es obligatorio." });
            if (nombre.Length > LargoMaximoNombre)
                return BadRequest(new { message = $"El nombre del contrato no puede superar los {LargoMaximoNombre} caracteres." });

            string? errorArchivo = ValidadorArchivos.Validar(dto.Archivo, ValidadorArchivos.SoloPdf);
            if (errorArchivo != null)
                return BadRequest(new { message = errorArchivo });

            Usuario? alumno = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == dto.IdUsuario && u.Alumnos.Any());
            if (alumno == null)
                return BadRequest(new { message = "La persona indicada no es un alumno." });

            int? idUsuarioCarga = User.ObtenerIdUsuario();
            if (idUsuarioCarga == null)
                return Unauthorized();

            // Nombre del archivo armado por el sistema, nunca el original del usuario.
            string nombreArchivo = string.Join("_",
                "ISCGB",
                ValidadorArchivos.LimpiarParaNombreArchivo($"{alumno.Nombre}{alumno.Apellido}"),
                ValidadorArchivos.LimpiarParaNombreArchivo(nombre),
                DateTime.Now.ToString("yyyyMMdd_HHmmss"),
                Guid.NewGuid().ToString("N")[..12]) + ".pdf";

            Directory.CreateDirectory(_carpetaContratos);
            string rutaFisica = Path.Combine(_carpetaContratos, nombreArchivo);

            using (FileStream stream = new FileStream(rutaFisica, FileMode.CreateNew))
            {
                await dto.Archivo!.CopyToAsync(stream);
            }

            ContratoAlumno contrato = new ContratoAlumno
            {
                IdUsuario = alumno.IdUsuario,
                Nombre = nombre,
                RutaArchivo = nombreArchivo,
                FechaCarga = DateTime.Now,
                IdUsuarioCarga = idUsuarioCarga.Value,
            };

            try
            {
                _context.ContratosAlumno.Add(contrato);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Si no se pudo registrar, no dejamos el archivo huérfano en el disco.
                System.IO.File.Delete(rutaFisica);
                throw;
            }

            return Ok(new { message = "Contrato cargado con éxito.", idContrato = contrato.IdContrato });
        }

        // GET: api/ContratosAlumno/alumno/{idUsuario} — contratos de un alumno
        [HttpGet("alumno/{idUsuario}")]
        [Authorize(Roles = UsuarioActual.RolesAdministrativos)]
        public async Task<ActionResult<List<ContratoAlumnoDto>>> DeAlumno(int idUsuario)
        {
            return await ListarAsync(idUsuario);
        }

        // GET: api/ContratosAlumno/mis-contratos — contratos del alumno de la sesión
        [HttpGet("mis-contratos")]
        [Authorize(Roles = "Alumno")]
        public async Task<ActionResult<List<ContratoAlumnoDto>>> MisContratos()
        {
            int? idUsuario = User.ObtenerIdUsuario();
            if (idUsuario == null)
                return Unauthorized();

            return await ListarAsync(idUsuario.Value);
        }

        // GET: api/ContratosAlumno/{idContrato}/pdf — el PDF, para Dirección,
        // Secretaría o el alumno dueño del contrato
        [HttpGet("{idContrato}/pdf")]
        public async Task<IActionResult> Pdf(int idContrato)
        {
            ContratoAlumno? contrato = await _context.ContratosAlumno.FindAsync(idContrato);
            if (contrato == null)
                return NotFound(new { message = "No se encontró el contrato." });

            if (!User.PuedeAccederA(contrato.IdUsuario))
                return Forbid();

            // Path.GetFileName: aunque la base tuviera una ruta rara, nunca sale de la carpeta.
            string rutaFisica = Path.Combine(_carpetaContratos, Path.GetFileName(contrato.RutaArchivo));
            if (!System.IO.File.Exists(rutaFisica))
                return NotFound(new { message = "El archivo del contrato no está disponible." });

            string nombreDescarga = $"{ValidadorArchivos.LimpiarParaNombreArchivo(contrato.Nombre)}.pdf";
            return PhysicalFile(rutaFisica, "application/pdf", nombreDescarga);
        }

        private async Task<List<ContratoAlumnoDto>> ListarAsync(int idUsuario)
        {
            return await _context.ContratosAlumno
                .Where(c => c.IdUsuario == idUsuario)
                .OrderByDescending(c => c.FechaCarga)
                .Select(c => new ContratoAlumnoDto
                {
                    IdContrato = c.IdContrato,
                    Nombre = c.Nombre,
                    FechaCarga = c.FechaCarga,
                })
                .ToListAsync();
        }
    }
}
