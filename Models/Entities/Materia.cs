using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models.Entities;

public partial class Materia
{
    public int IdMateria { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Carrera { get; set; }

    public string? Curso { get; set; }

    public virtual ICollection<AlumnoMaterium> AlumnoMateria { get; set; } = new List<AlumnoMaterium>();

    public virtual ICollection<Correlatividade> CorrelatividadeCorrelativas { get; set; } = new List<Correlatividade>();

    public virtual ICollection<Correlatividade> CorrelatividadeMateria { get; set; } = new List<Correlatividade>();

    public virtual ICollection<DocenteMaterium> DocenteMateria { get; set; } = new List<DocenteMaterium>();

    public virtual ICollection<Examene> Examenes { get; set; } = new List<Examene>();

    public virtual ICollection<PlanesMateria> PlanesMateria { get; set; } = new List<PlanesMateria>();

    public virtual ICollection<ProgramasMaterium> ProgramasMateria { get; set; } = new List<ProgramasMaterium>();

    public virtual ICollection<ReconocimientoSabere> ReconocimientoSaberes { get; set; } = new List<ReconocimientoSabere>();
}
