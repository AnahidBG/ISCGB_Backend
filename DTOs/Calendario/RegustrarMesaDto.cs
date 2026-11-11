namespace AutoGestionAPI.DTOs
{
    /// <summary>
    /// Alta de una mesa de examen con su tribunal completo: presidente y dos vocales
    /// (tres docentes distintos). A los tres les llega el aviso, y la mesa aparece en
    /// el calendario cuando los tres aceptan.
    ///
    /// La mesa es de la materia (y por lo tanto de su año) para TODAS las comisiones,
    /// y siempre queda con el tipo "Mesa de Examen".
    /// </summary>
    public class RegistrarMesaDto
    {
        public DateTime Fecha { get; set; }
        public int IdMateria { get; set; }

        /// <summary>Se ignora: la mesa es para todas las comisiones. Se acepta para no romper el front actual.</summary>
        public int? IdComision { get; set; }

        /// <summary>Se ignora: la mesa siempre queda como "Mesa de Examen". Se acepta para no romper el front actual.</summary>
        public int? IdTipoExamen { get; set; }

        public int IdPresidente { get; set; }
        public int IdVocal1 { get; set; }
        public int IdVocal2 { get; set; }
    }
}
