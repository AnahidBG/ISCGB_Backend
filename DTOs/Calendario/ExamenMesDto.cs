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

        /// <summary>
        /// Si el usuario logueado puede modificarlo o eliminarlo: Dirección/Secretaría
        /// siempre; un docente, solo si lo cargó él (Parcial, Recuperatorio o Final).
        /// </summary>
        public bool PuedeModificar { get; set; }
    }
}