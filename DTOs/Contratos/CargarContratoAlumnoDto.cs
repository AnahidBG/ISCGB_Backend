using Microsoft.AspNetCore.Http;

namespace AutoGestionAPI.DTOs.Contratos
{
    /// <summary>multipart/form-data que manda la pantalla "Contratos de alumnos".</summary>
    public class CargarContratoAlumnoDto
    {
        /// <summary>El usuario con rol Alumno al que se le carga el contrato.</summary>
        public int IdUsuario { get; set; }

        /// <summary>Nombre visible (hasta 100 caracteres).</summary>
        public string? Nombre { get; set; }

        /// <summary>El PDF del contrato firmado.</summary>
        public IFormFile? Archivo { get; set; }
    }

    /// <summary>Un contrato en los listados: <c>{ idContrato, nombre, fechaCarga }</c>.</summary>
    public class ContratoAlumnoDto
    {
        public int IdContrato { get; set; }
        public string Nombre { get; set; } = null!;
        public DateTime FechaCarga { get; set; }
    }
}
