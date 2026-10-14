using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoGestionAPI.Models.Entities;

public partial class Correlatividade
{
    public int Id { get; set; }

    public int PlanEstudioId { get; set; }

    public int MateriaId { get; set; }

    public int CorrelativaId { get; set; }
[NotMapped]
    public virtual Materia Correlativa { get; set; } = null!;

    public virtual Materia Materia { get; set; } = null!;

    public virtual PlanesEstudio PlanEstudio { get; set; } = null!;
}
