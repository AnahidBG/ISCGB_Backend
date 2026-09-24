using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models;

public partial class PlanesMateria
{
    public int Id { get; set; }

    public int Año { get; set; }

    public int Cuatrimestre { get; set; }

    public int PlanEstudioId { get; set; }

    public int MateriaId { get; set; }

    public virtual PlanesEstudio PlanEstudio { get; set; } = null!;
}
