public class MateriaDocenteDto
{
    public int IdMateria { get; set; }
    public string Nombre { get; set; }
    public string? Carrera { get; set; }
    public string? Curso { get; set; }
    public int? NroOrden { get; set; }
    public string? Formato { get; set; }
    public int? HorasCatedra { get; set; }
    public int? HorasTotales { get; set; }

    public int IdComision { get; set; }
}