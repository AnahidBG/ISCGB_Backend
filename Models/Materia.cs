using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models;

public partial class Materia
{
    public int IdMateria { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Carrera { get; set; }

    public string? Curso { get; set; }

    public virtual ICollection<AlumnoMaterium> AlumnoMateria { get; set; } = new List<AlumnoMaterium>();

    public virtual ICollection<DocenteMaterium> DocenteMateria { get; set; } = new List<DocenteMaterium>();

    public virtual ICollection<Examene> Examenes { get; set; } = new List<Examene>();

    public virtual ICollection<ProgramasMaterium> ProgramasMateria { get; set; } = new List<ProgramasMaterium>();

    public virtual ICollection<ReconocimientoSabere> ReconocimientoSaberes { get; set; } = new List<ReconocimientoSabere>();
}
