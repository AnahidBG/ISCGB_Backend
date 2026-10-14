namespace AutoGestionAPI.DTOs
{
    public class ActualizarPerfilDto
    {
        public string? Telefono { get; set; }
        public string? TelefonoEmergencia { get; set; }
        public string? LugarNacimiento { get; set; }
        public string? ContactoEmergencia { get; set; }
        public string? Direccion { get; set; }
        public int? IdProvincia { get; set; }
        public string? Genero { get; set; }
        public string? AfiliacionEmergencia { get; set; }
        public string? Email { get; set; }
    }
}