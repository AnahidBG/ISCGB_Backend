using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models.Entities;

public partial class PlanesEstudio
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public int AñoInicio { get; set; }

    public int AñoFin { get; set; }

    public string Estado { get; set; } = null!;

    public virtual ICollection<Correlatividade> Correlatividades { get; set; } = new List<Correlatividade>();

    public virtual ICollection<PlanesMateria> PlanesMateria { get; set; } = new List<PlanesMateria>();
}
