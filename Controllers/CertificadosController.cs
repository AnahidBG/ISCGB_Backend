using AutoGestionAPI.Models;
using AutoGestionAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoGestionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class CertificadosController : ControllerBase
    {
        private readonly TuDbContext _context;

        public CertificadosController(TuDbContext context)
        {
            _context = context;
        }

        [HttpGet("alumno-regular")]
        public IActionResult GenerarCertificadoAlumnoRegular()
        {
            return GenerarCertificado(false);
        }

        [HttpGet("alumno-regular-horario")]
        public IActionResult GenerarCertificadoAlumnoRegularConHorario()
        {
            return GenerarCertificado(true);
        }

        private IActionResult GenerarCertificado(bool conHorario)
        {
            /*string? idUsuarioClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;*/

                string? idUsuarioClaim = "6";
                //POINER ID

            if (string.IsNullOrEmpty(idUsuarioClaim))
            {
                return Unauthorized(new
                {
                    message = "No se pudo identificar al usuario."
                });
            }

            if (!int.TryParse(idUsuarioClaim, out int idUsuario))
            {
                return Unauthorized(new
                {
                    message = "El identificador del usuario no es válido."
                });
            }

            Usuario? usuario = _context.Usuarios
                .FirstOrDefault(u => u.IdUsuario == idUsuario);

            if (usuario == null)
            {
                return NotFound(new
                {
                    message = "No se encontró el usuario."
                });
            }

            Alumno? alumno = _context.Alumnos
                .FirstOrDefault(a => a.IdUsuario == idUsuario);

            /*if (alumno == null)
            {
                return NotFound(new
                {
                    message = "El usuario no corresponde a un alumno."
                });
            }*/

            if (string.IsNullOrWhiteSpace(usuario.Nombre) ||
                string.IsNullOrWhiteSpace(usuario.Apellido) ||
                string.IsNullOrWhiteSpace(usuario.Dni))
            {
                return BadRequest(new
                {
                    message = "El alumno no tiene completos los datos necesarios."
                });
            }

            string nombreCompleto =
                $"{usuario.Nombre} {usuario.Apellido}";

            byte[] pdf = GeneradorPDFCertificado.CrearCertificado(
                nombreCompleto,
                usuario.Dni,
                conHorario);

            string nombreArchivo;

            if (conHorario)
            {
                nombreArchivo =
                    "Certificado_Alumno_Regular_Horario.pdf";
            }
            else
            {
                nombreArchivo =
                    "Certificado_Alumno_Regular.pdf";
            }

            return File(
                pdf,
                "application/pdf",
                nombreArchivo);
        }
    }
}