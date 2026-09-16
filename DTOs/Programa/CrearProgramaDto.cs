namespace AutoGestionAPI.DTOs
{
    // 1. DTO para las unidades (Contenidos)
    public class CrearContenidoDto
    {
        public int Unidad { get; set; }
        public string? TituloUnidad { get; set; }
        public string? Contenido { get; set; }
        public string? BibliografiaObligatoria { get; set; }
        public string? BibliografiaComplementaria { get; set; }
    }

    public class CrearProgramaDto
    {
        public int IdDocente { get; set; }
        public int IdMateria { get; set; }
        public string? Condicion { get; set; }
        public string? ObjetivosEspecificos { get; set; }
        public string? ObjetivosGenerales { get; set; }
        public string? HorasSemanales { get; set; }
        public string? HorasCuatrimestrales { get; set; }
        public string? Evaluacion { get; set; }
        public string? CriteriosEvaluacion { get; set; }
        public string? EstrategiasMetodologicas { get; set; }
        public string? EstrategiasAcompanamientoVirtualRemoto { get; set; }
        public string? CondicionRegular { get; set; }
        public string? CondicionPromocional { get; set; }
        public string? CondicionLibre { get; set; }
        public string? ExamenesVirtuales { get; set; }
        public string? FormatoCurricular { get; set; }
        public string? CicloLectivo { get; set; }
        public string? Fundamentacion { get; set; }

        public List<CrearContenidoDto> Contenidos { get; set; } = new List<CrearContenidoDto>();
    }
}