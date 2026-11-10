namespace AutoGestionAPI.DTOs
{
    /// <summary>
    /// Alta de una mesa de examen con su tribunal completo: presidente y dos vocales
    /// (tres docentes distintos). A los tres les llega el aviso, y la mesa aparece en
    /// el calendario cuando los tres aceptan.
    /// </summary>
    public class RegistrarMesaDto
    {
        public DateTime Fecha { get; set; }
        public int IdMateria { get; set; }
        public int IdComision { get; set; }
        public int IdTipoExamen { get; set; } // Aquí el frontend enviará el ID correspondiente a "Final"

        public int IdPresidente { get; set; }
        public int IdVocal1 { get; set; }
        public int IdVocal2 { get; set; }
    }
}
