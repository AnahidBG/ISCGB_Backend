using System.ComponentModel.DataAnnotations;

namespace AutoGestionAPI.DTOs
{
    public class ActualizarPerfilDto
    {
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        [MaxLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres.")]
        public string? Email { get; set; }

        [MaxLength(30, ErrorMessage = "El teléfono no puede superar los 30 caracteres.")]
        public string? Telefono { get; set; }

        [MaxLength(30, ErrorMessage = "El teléfono de emergencia no puede superar los 30 caracteres.")]
        public string? TelefonoEmergencia { get; set; }


        public string? LugarNacimiento { get; set; }

        [MaxLength(150, ErrorMessage = "El contacto de emergencia no puede superar los 150 caracteres.")]
        public string? ContactoEmergencia { get; set; }

        [MaxLength(255, ErrorMessage = "La dirección no puede superar los 255 caracteres.")]
        public string? Direccion { get; set; }

        public int? IdProvincia { get; set; }

        [MaxLength(20, ErrorMessage = "El género no puede superar los 20 caracteres.")]
        public string? Genero { get; set; }

        [MaxLength(100, ErrorMessage = "La afiliación de emergencia no puede superar los 100 caracteres.")]
        public string? AfiliacionEmergencia { get; set; }
    }
}