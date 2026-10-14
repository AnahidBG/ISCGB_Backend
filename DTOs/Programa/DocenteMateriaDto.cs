namespace AutoGestionAPI.DTOs
{
    public class ContextoDocenteDto
    {
        public int IdDocente { get; set; }
        public List<MateriaDocenteDto> Materias { get; set; } = new List<MateriaDocenteDto>();
    }
}