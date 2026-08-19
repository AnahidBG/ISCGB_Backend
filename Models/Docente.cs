using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models;

public partial class Docente
{
    public int IdDocente { get; set; }

    public int IdUsuario { get; set; }

    public bool? DirectorSuplente { get; set; }

    public virtual ICollection<DocenteMaterium> DocenteMateria { get; set; } = new List<DocenteMaterium>();

    public virtual ICollection<DocenteTipoTitulo> DocenteTipoTitulos { get; set; } = new List<DocenteTipoTitulo>();

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual ICollection<ProgramasMaterium> ProgramasMateria { get; set; } = new List<ProgramasMaterium>();

    public virtual ICollection<ReconocimientoSabere> ReconocimientoSaberes { get; set; } = new List<ReconocimientoSabere>();

    public virtual ICollection<Examene> IdExamen { get; set; } = new List<Examene>();
}
