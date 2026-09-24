using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models;

public partial class Correlatividade
{
    public int Id { get; set; }

    public int PlanEstudioId { get; set; }

    public int MateriaId { get; set; }

    public int CorrelativaId { get; set; }

    public virtual PlanesEstudio PlanEstudio { get; set; } = null!;
}
