namespace AutoGestionAPI.DTOs
{
    public class CargaExamenDto
    {
        public DateTime Fecha { get; set; }
        public int IdComision { get; set; }
        public int IdMateria { get; set; }
        public int IdTipoExamen { get; set; } // El ID que en su base de datos representa a "Parcial"
    }
}