namespace AutoGestionAPI.DTOs.Usuarios
{
    public class CargaUsuarioDto
    {
        // Sección Datos Personales
        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public string Dni { get; set; } = null!;
        public string Cuil { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Genero { get; set; } = null!;
        public string Direccion { get; set; } = null!;
        public string Telefono { get; set; } = null!;
        public int? IdProvincia { get; set; }
        public DateOnly? FechaNac { get; set; }

        // Sección Contacto de Emergencia
        public string ContactoEmergencia { get; set; } = null!;
        public string TelefonoEmergencia { get; set; } = null!;
        public string AfiliacionEmergencia { get; set; } = null!;

        // Sección Información Académica
        public int? IdRol { get; set; } // 1: Director, 2: Secretario, 3: Docente, 4: Alumno
        public bool EsDirectorSuplente { get; set; } // Solo para Docentes
    }
}