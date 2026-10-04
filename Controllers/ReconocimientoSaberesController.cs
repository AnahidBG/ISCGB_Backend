using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using AutoGestionAPI.Models;
using AutoGestionAPI.DTOs;

namespace AutoGestionAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReconocimientoSaberesController : ControllerBase
    {
        private readonly TuDbContext _context;
        //Los archivos se guardan dentro de: wwwroot/uploads/reconocimientos
        private readonly string _uploadPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot", "uploads", "reconocimientos");

        public ReconocimientoSaberesController(TuDbContext context)
        {
            _context = context;
            if (!Directory.Exists(_uploadPath))
                Directory.CreateDirectory(_uploadPath);
        }

        // Alumno envia solicitud de reconocimiento
        [HttpPost("solicitar")]
        [Authorize(Roles = "Alumno")]
        public async Task<IActionResult> EnviarSolicitud([FromForm] SolicitudReconocimientoDto dto)
        {
            // Validación de PDFs
            if (!EsPdfValido(dto.ProgramaPdf) || !EsPdfValido(dto.AnaliticoPdf))
                return BadRequest("Ambos archivos deben ser formato PDF y pesar un máximo de 10 MB.");

            // Identificar al alumno autenticado
            string? usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (!int.TryParse(usuarioIdClaim, out int idUsuario))
                return Unauthorized("Token inválido.");


            var alumno = await _context.Alumnos.FirstOrDefaultAsync(a => a.IdUsuario == idUsuario);
            if (alumno == null)
                return BadRequest("El usuario no es un alumno.");

            // Primero guarda en Base de Datos primero para obtener el id_solicitud
            ReconocimientoSabere nuevaSolicitud = new ReconocimientoSabere
            {
                IdMateria = dto.IdMateria,
                IdAlumno = alumno.IdAlumno,
                Comentario = dto.Comentario,
                IdDocente = null
            };

            _context.ReconocimientoSaberes.Add(nuevaSolicitud);
            await _context.SaveChangesAsync();

            try
            {
                // Despues guarda los archivos pdf
                string programaFileName = $"programa_{nuevaSolicitud.IdSolicitud}.pdf";
                string analiticoFileName = $"analitico_{nuevaSolicitud.IdSolicitud}.pdf";

                string programaPath = Path.Combine(_uploadPath, programaFileName);
                string analiticoPath = Path.Combine(_uploadPath, analiticoFileName);

                //Guarda programa de materia
                using (FileStream stream = new FileStream(programaPath, FileMode.Create))
                    await dto.ProgramaPdf.CopyToAsync(stream);

                //Guarda analitico del alumno
                using (FileStream stream = new FileStream(analiticoPath, FileMode.Create))
                    await dto.AnaliticoPdf.CopyToAsync(stream);

                //Si sale Ok
                return Ok(new { mensaje = "Solicitud enviada a Secretaría/Preceptoria con éxito." });
            }
            catch
            {
                // Si ocurre un error al guardar los archivos, elimina también la solicitud de la BBDD
                // para evitar dejar una solicitud incompleta.

                string programaPath = Path.Combine(_uploadPath, $"programa_{nuevaSolicitud.IdSolicitud}.pdf");

                string analiticoPath = Path.Combine(
                    _uploadPath,
                    $"analitico_{nuevaSolicitud.IdSolicitud}.pdf");

                if (System.IO.File.Exists(programaPath))
                {
                    System.IO.File.Delete(programaPath);
                }

                if (System.IO.File.Exists(analiticoPath))
                {
                    System.IO.File.Delete(analiticoPath);
                }

                _context.ReconocimientoSaberes.Remove(nuevaSolicitud);

                await _context.SaveChangesAsync();

                return StatusCode(
                    500,
                    "No se pudo guardar la documentación de la solicitud.");

            }
        }

            //Secretaria recibe la solicitud
            [HttpGet("recibirSolicitudReconocimiento")]
            [Authorize(Roles = "Secretario")]
            public async Task<IActionResult> ObtenerPendientesSecretaria()
            {
                var solicitudes = await (from r in _context.ReconocimientoSaberes
                                         join a in _context.Alumnos on r.IdAlumno equals a.IdAlumno // Relaciona solicitud con alumno
                                         join u in _context.Usuarios on a.IdUsuario equals u.IdUsuario // para obtener nombre, apellido y DNI
                                         join m in _context.Materias on r.IdMateria equals m.IdMateria // Relaciona solicitud con materia
                                         where r.IdDocente == null

                                         select new
                                         {
                                             IdSolicitud = r.IdSolicitud,
                                             AlumnoNombreCompleto = u.Apellido + ", " + u.Nombre,
                                             DNI = u.Dni,
                                             MateriaSolicitada = m.Nombre,
                                             Comentario = r.Comentario,

                                             // rutas URL para que Angular las pueda descargar
                                             UrlProgramaPdf =
                                                $"/api/ReconocimientoSaberes/{r.IdSolicitud}/programa",

                                             UrlAnaliticoPdf =
                                                $"/api/ReconocimientoSaberes/{r.IdSolicitud}/analitico"
                                         }).ToListAsync();

                return Ok(solicitudes);
            }

            //Secretario obtiene detalle de la solicitud
            [HttpGet("{id}")]
            [Authorize(Roles = "Secretario")]
            public async Task<IActionResult> ObtenerSolicitud(int id)
            {
                var solicitud = await (
                    from r in _context.ReconocimientoSaberes

                    join a in _context.Alumnos
                        on r.IdAlumno equals a.IdAlumno

                    join u in _context.Usuarios
                        on a.IdUsuario equals u.IdUsuario

                    join m in _context.Materias
                        on r.IdMateria equals m.IdMateria

                    where r.IdSolicitud == id

                    select new
                    {
                        IdSolicitud = r.IdSolicitud,

                        Nombre = u.Nombre,

                        Apellido = u.Apellido,

                        DNI = u.Dni,

                        Materia = m.Nombre,

                        Comentario = r.Comentario,

                        UrlProgramaPdf =
                            $"/api/ReconocimientoSaberes/{r.IdSolicitud}/programa",

                        UrlAnaliticoPdf =
                            $"/api/ReconocimientoSaberes/{r.IdSolicitud}/analitico"
                    })
                    .FirstOrDefaultAsync();

                if (solicitud == null)
                {
                    return NotFound("No se encontró la solicitud.");
                }

                return Ok(solicitud);
            }

            //Secretario ve o descarga el programa - Devuelve el PDF del programa correspondiente
            [HttpGet("{id}/programa")]
            [Authorize(Roles = "Secretario")]
            public IActionResult ObtenerPrograma(int id)
            {
                string ruta = Path.Combine(
                    _uploadPath,
                    $"programa_{id}.pdf");

                // Verifica que el archivo exista.
                if (!System.IO.File.Exists(ruta))
                {
                    return NotFound(
                        "No se encontró el programa de la solicitud.");
                }

                // Devuelve el archivo como PDF.
                return PhysicalFile(
                    ruta,
                    "application/pdf", $"programa_{id}.pdf");
            }

            //Secretario descarga anualitico
            [HttpGet("{id}/analitico")]
            [Authorize(Roles = "Secretario")]
            public IActionResult ObtenerAnalitico(int id)
            {
                string ruta = Path.Combine(
                    _uploadPath,
                    $"analitico_{id}.pdf");

                // Verifica que el archivo exista.
                if (!System.IO.File.Exists(ruta))
                {
                    return NotFound(
                        "No se encontró el analítico de la solicitud.");
                }

                // Devuelve el archivo como PDF.
                return PhysicalFile(
                    ruta,
                    "application/pdf", $"analitico_{id}.pdf");
            }

        // Valida los archivos
        private bool EsPdfValido(IFormFile archivo)
        {
            long limite = 10 * 1024 * 1024; // 10 MB
            return archivo != null && archivo.ContentType == "application/pdf" && archivo.Length <= limite && archivo.Length > 0;
        }
    }
}