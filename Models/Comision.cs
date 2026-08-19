using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models;

public partial class Comision
{
    public int IdComision { get; set; }

    public string Comision1 { get; set; } = null!;

    public virtual ICollection<AlumnoMaterium> AlumnoMateria { get; set; } = new List<AlumnoMaterium>();

    public virtual ICollection<DocenteMaterium> DocenteMateria { get; set; } = new List<DocenteMaterium>();

    public virtual ICollection<Examene> Examenes { get; set; } = new List<Examene>();
}
