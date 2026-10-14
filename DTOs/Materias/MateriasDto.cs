public class MateriaDocenteDto
{
    public int IdMateria { get; set; }
    public string Nombre { get; set; }
    public string? Carrera { get; set; }
    public string? Curso { get; set; }

    public int IdComision { get; set; }

    public string NombreComision { get; set; } = null!;
}