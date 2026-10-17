namespace AutoGestionAPI.DTOs
{
    public class ExamenMesDto
    {
        public int IdExamen { get; set; }
        public string Materia { get; set; } = null!;
        public DateTime Fecha { get; set; }

        // --- NUEVOS CAMPOS ---
        public string Comision { get; set; } = null!;
        public string TipoExamen { get; set; } = null!;
    }
}