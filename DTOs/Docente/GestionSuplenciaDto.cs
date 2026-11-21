namespace AutoGestionAPI.DTOs
{
    public class GestionarSuplenciaDto
    {
        public bool EsSuplente { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }

        /// <summary>
        /// Al activar la suplencia: el docente titular al que reemplaza. Sus materias
        /// pasan al suplente mientras dure la suplencia y vuelven al titular cuando se
        /// da de baja. Opcional: sin él, la suplencia no mueve materias.
        /// </summary>
        public int? IdDocenteTitular { get; set; }
    }
}
