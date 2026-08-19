using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models;

public partial class ProgramasMaterium
{
    public int IdPrograma { get; set; }

    public int IdDocente { get; set; }

    public int IdMateria { get; set; }

    public string? Condicion { get; set; }

    public string? ObjetivosEspecificos { get; set; }

    public string? ObjetivosGenerales { get; set; }

    public string? HorasSemanales { get; set; }

    public string? HorasCuatrimestrales { get; set; }

    public string? Evaluacion { get; set; }

    public string? CriteriosEvaluacion { get; set; }

    public string? EstrategiasMetodologicas { get; set; }

    public string? EstrategiasAcompanamientoVirtualRemoto { get; set; }

    public string? CondicionRegular { get; set; }

    public string? CondicionPromocional { get; set; }

    public string? CondicionLibre { get; set; }

    public string? ExamenesVirtuales { get; set; }

    public string? FormatoCurricular { get; set; }

    public string? CicloLectivo { get; set; }

    public virtual ICollection<Contenido> Contenidos { get; set; } = new List<Contenido>();

    public virtual Docente IdDocenteNavigation { get; set; } = null!;

    public virtual Materia IdMateriaNavigation { get; set; } = null!;
}
