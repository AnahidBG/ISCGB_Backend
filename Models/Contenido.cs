using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models;

public partial class Contenido
{
    public int IdContenido { get; set; }

    public int IdPrograma { get; set; }

    public int Unidad { get; set; }

    public string? TituloUnidad { get; set; }

    public string? Contenido1 { get; set; }

    public string? BibliografiaObligatoria { get; set; }

    public string? BibliografiaComplementaria { get; set; }

    public virtual ProgramasMaterium IdProgramaNavigation { get; set; } = null!;
}
