namespace AutoGestionAPI.DTOs
{
    public class RegistrarMesaDto
    {
        public DateTime Fecha { get; set; }
        public int IdMateria { get; set; }
        public int IdComision { get; set; }
        public int IdTipoExamen { get; set; } // Aquí el frontend enviará el ID correspondiente a "Final" o "Mesa"
        public int IdDocente { get; set; }    // El profesor titular o vocal que se está convocando
    }
}