using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace AutoGestionAPI.Models;

public partial class AutogestionDocenteContext : DbContext
{
    public AutogestionDocenteContext()
    {
    }

    public AutogestionDocenteContext(DbContextOptions<AutogestionDocenteContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Alumno> Alumnos { get; set; }

    public virtual DbSet<AlumnoMaterium> AlumnoMateria { get; set; }

    public virtual DbSet<Comision> Comisions { get; set; }

    public virtual DbSet<Contenido> Contenidos { get; set; }

    public virtual DbSet<Correlatividade> Correlatividades { get; set; }

    public virtual DbSet<Docente> Docentes { get; set; }

    public virtual DbSet<DocenteMaterium> DocenteMateria { get; set; }

    public virtual DbSet<DocenteTipoTitulo> DocenteTipoTitulos { get; set; }

    public virtual DbSet<Examene> Examenes { get; set; }

    public virtual DbSet<Justificativo> Justificativos { get; set; }

    public virtual DbSet<Legajo> Legajos { get; set; }

    public virtual DbSet<Materia> Materias { get; set; }

    public virtual DbSet<Pai> Pais { get; set; }

    public virtual DbSet<PlanesEstudio> PlanesEstudios { get; set; }

    public virtual DbSet<PlanesMateria> PlanesMaterias { get; set; }

    public virtual DbSet<ProgramasMaterium> ProgramasMateria { get; set; }

    public virtual DbSet<Provincium> Provincia { get; set; }

    public virtual DbSet<ReconocimientoSabere> ReconocimientoSaberes { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolesTiposDocumento> RolesTiposDocumentos { get; set; }

    public virtual DbSet<TipoExaman> TipoExamen { get; set; }

    public virtual DbSet<TipoTitulo> TipoTitulos { get; set; }

    public virtual DbSet<TiposDocumento> TiposDocumentos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<UsuariosRole> UsuariosRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-TFTN2H0\\ANAHIDBG;Database=Autogestion_Docente;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alumno>(entity =>
        {
            entity.HasKey(e => e.IdAlumno).HasName("PK__Alumnos__6D77A7F1481826C1");

            entity.Property(e => e.IdAlumno).HasColumnName("id_alumno");
            entity.Property(e => e.Cohorte)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("cohorte");
            entity.Property(e => e.EstadoAcademico)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("estado_academico");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Alumnos)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Alumnos_Usuario");
        });

        modelBuilder.Entity<AlumnoMaterium>(entity =>
        {
            entity.HasKey(e => e.IdAlumnoMateria).HasName("PK__alumno_m__45915319D2DB8C4E");

            entity.ToTable("alumno_materia");

            entity.Property(e => e.IdAlumnoMateria).HasColumnName("id_alumno_materia");
            entity.Property(e => e.IdAlumno).HasColumnName("id_alumno");
            entity.Property(e => e.IdComision).HasColumnName("id_comision");
            entity.Property(e => e.IdMateria).HasColumnName("id_materia");

            entity.HasOne(d => d.IdAlumnoNavigation).WithMany(p => p.AlumnoMateria)
                .HasForeignKey(d => d.IdAlumno)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AlumnoMateria_Alumno");

            entity.HasOne(d => d.IdComisionNavigation).WithMany(p => p.AlumnoMateria)
                .HasForeignKey(d => d.IdComision)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AlumnoMateria_Comision");

            entity.HasOne(d => d.IdMateriaNavigation).WithMany(p => p.AlumnoMateria)
                .HasForeignKey(d => d.IdMateria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AlumnoMateria_Materia");
        });

        modelBuilder.Entity<Comision>(entity =>
        {
            entity.HasKey(e => e.IdComision).HasName("PK__comision__B25ABED0F5189157");

            entity.ToTable("comision");

            entity.Property(e => e.IdComision).HasColumnName("id_comision");
            entity.Property(e => e.Comision1)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("comision");
        });

        modelBuilder.Entity<Contenido>(entity =>
        {
            entity.HasKey(e => e.IdContenido).HasName("PK__contenid__2A39B2EB2DAA2E2E");

            entity.ToTable("contenidos");

            entity.Property(e => e.IdContenido).HasColumnName("id_contenido");
            entity.Property(e => e.BibliografiaComplementaria)
                .IsUnicode(false)
                .HasColumnName("bibliografia_complementaria");
            entity.Property(e => e.BibliografiaObligatoria)
                .IsUnicode(false)
                .HasColumnName("bibliografia_obligatoria");
            entity.Property(e => e.Contenido1)
                .IsUnicode(false)
                .HasColumnName("contenido");
            entity.Property(e => e.IdPrograma).HasColumnName("id_programa");
            entity.Property(e => e.TituloUnidad)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("titulo_unidad");
            entity.Property(e => e.Unidad).HasColumnName("unidad");

            entity.HasOne(d => d.IdProgramaNavigation).WithMany(p => p.Contenidos)
                .HasForeignKey(d => d.IdPrograma)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Contenidos_Programa");
        });

        modelBuilder.Entity<Correlatividade>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Correlat__3214EC07F66D79B4");

            entity.HasOne(d => d.Correlativa).WithMany(p => p.CorrelatividadeCorrelativas)
                .HasForeignKey(d => d.CorrelativaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Correlatividad_Correlativa");

            entity.HasOne(d => d.Materia).WithMany(p => p.CorrelatividadeMateria)
                .HasForeignKey(d => d.MateriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Correlatividad_Materia");

            entity.HasOne(d => d.PlanEstudio).WithMany(p => p.Correlatividades)
                .HasForeignKey(d => d.PlanEstudioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Correlatividad_PlanEstudio");
        });

        modelBuilder.Entity<Docente>(entity =>
        {
            entity.HasKey(e => e.IdDocente).HasName("PK__Docentes__300DB2117C756E44");

            entity.Property(e => e.IdDocente).HasColumnName("id_docente");
            entity.Property(e => e.DirectorSuplente)
                .HasDefaultValue(false)
                .HasColumnName("director_suplente");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Docentes)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Docentes_Usuario");
        });

        modelBuilder.Entity<DocenteMaterium>(entity =>
        {
            entity.HasKey(e => e.IdDocenteMateria).HasName("PK__docente___2FCCA38F724420F2");

            entity.ToTable("docente_materia");

            entity.Property(e => e.IdDocenteMateria).HasColumnName("id_docente_materia");
            entity.Property(e => e.IdComision).HasColumnName("id_comision");
            entity.Property(e => e.IdDocente).HasColumnName("id_docente");
            entity.Property(e => e.IdMateria).HasColumnName("id_materia");

            entity.HasOne(d => d.IdComisionNavigation).WithMany(p => p.DocenteMateria)
                .HasForeignKey(d => d.IdComision)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocenteMateria_Comision");

            entity.HasOne(d => d.IdDocenteNavigation).WithMany(p => p.DocenteMateria)
                .HasForeignKey(d => d.IdDocente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocenteMateria_Docente");

            entity.HasOne(d => d.IdMateriaNavigation).WithMany(p => p.DocenteMateria)
                .HasForeignKey(d => d.IdMateria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocenteMateria_Materia");
        });

        modelBuilder.Entity<DocenteTipoTitulo>(entity =>
        {
            entity.HasKey(e => e.IdDocenteTipoTitulo).HasName("PK__docente___7568C5C0E9726EE2");

            entity.ToTable("docente_tipo_titulo");

            entity.Property(e => e.IdDocenteTipoTitulo).HasColumnName("id_docente_tipo_titulo");
            entity.Property(e => e.FechaEgreso)
                .HasColumnType("datetime")
                .HasColumnName("fecha_egreso");
            entity.Property(e => e.IdDocente).HasColumnName("id_docente");
            entity.Property(e => e.IdTipoTitulo).HasColumnName("id_tipo_titulo");
            entity.Property(e => e.Institucion)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("institucion");

            entity.HasOne(d => d.IdDocenteNavigation).WithMany(p => p.DocenteTipoTitulos)
                .HasForeignKey(d => d.IdDocente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocenteTipoTitulo_Docente");

            entity.HasOne(d => d.IdTipoTituloNavigation).WithMany(p => p.DocenteTipoTitulos)
                .HasForeignKey(d => d.IdTipoTitulo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DocenteTipoTitulo_TipoTitulo");
        });

        modelBuilder.Entity<Examene>(entity =>
        {
            entity.HasKey(e => e.IdExamen).HasName("PK__Examenes__D16A231D25C591AF");

            entity.Property(e => e.IdExamen).HasColumnName("id_examen");
            entity.Property(e => e.Fecha)
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdComision).HasColumnName("id_comision");
            entity.Property(e => e.IdMateria).HasColumnName("id_materia");
            entity.Property(e => e.IdTipoExamen).HasColumnName("id_tipo_examen");

            entity.HasOne(d => d.IdComisionNavigation).WithMany(p => p.Examenes)
                .HasForeignKey(d => d.IdComision)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Examenes_Comision");

            entity.HasOne(d => d.IdMateriaNavigation).WithMany(p => p.Examenes)
                .HasForeignKey(d => d.IdMateria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Examenes_Materia");

            entity.HasOne(d => d.IdTipoExamenNavigation).WithMany(p => p.Examenes)
                .HasForeignKey(d => d.IdTipoExamen)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Examenes_TipoExamen");

            entity.HasMany(d => d.IdDocentes).WithMany(p => p.IdExamen)
                .UsingEntity<Dictionary<string, object>>(
                    "MesaExaman",
                    r => r.HasOne<Docente>().WithMany()
                        .HasForeignKey("IdDocente")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_MesaExamen_Docente"),
                    l => l.HasOne<Examene>().WithMany()
                        .HasForeignKey("IdExamen")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_MesaExamen_Examen"),
                    j =>
                    {
                        j.HasKey("IdExamen", "IdDocente");
                        j.ToTable("mesa_examen");
                        j.IndexerProperty<int>("IdExamen").HasColumnName("id_examen");
                        j.IndexerProperty<int>("IdDocente").HasColumnName("id_docente");
                    });
        });

        modelBuilder.Entity<Justificativo>(entity =>
        {
            entity.HasKey(e => e.IdJustificativo).HasName("PK__Justific__C281B796BC132338");

            entity.Property(e => e.IdJustificativo).HasColumnName("id_justificativo");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.FechaCarga)
                .HasColumnType("datetime")
                .HasColumnName("fecha_carga");
            entity.Property(e => e.FechaInasistenciaFin)
                .HasColumnType("datetime")
                .HasColumnName("fecha_inasistencia_fin");
            entity.Property(e => e.FechaInasistenciaInicio)
                .HasColumnType("datetime")
                .HasColumnName("fecha_inasistencia_inicio");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.IdUsuarioAuditor).HasColumnName("id_usuario_auditor");
            entity.Property(e => e.NotaAdicional)
                .IsUnicode(false)
                .HasColumnName("nota_adicional");
            entity.Property(e => e.RutaArchivo)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("ruta_archivo");
            entity.Property(e => e.TipoInasistencia)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("tipo_inasistencia");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.JustificativoIdUsuarioNavigations)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Justificativos_Usuario");

            entity.HasOne(d => d.IdUsuarioAuditorNavigation).WithMany(p => p.JustificativoIdUsuarioAuditorNavigations)
                .HasForeignKey(d => d.IdUsuarioAuditor)
                .HasConstraintName("FK_Justificativos_Auditor");
        });

        modelBuilder.Entity<Legajo>(entity =>
        {
            entity.HasKey(e => e.IdLegajo).HasName("PK__legajo__AB7BD83C9E6C27DD");

            entity.ToTable("legajo");

            entity.Property(e => e.IdLegajo).HasColumnName("id_legajo");
            entity.Property(e => e.Comentario)
                .IsUnicode(false)
                .HasColumnName("comentario");
            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.FechaCarga)
                .HasColumnType("datetime")
                .HasColumnName("fecha_carga");
            entity.Property(e => e.FechaVencimiento)
                .HasColumnType("datetime")
                .HasColumnName("fecha_vencimiento");
            entity.Property(e => e.IdTipoDoc).HasColumnName("id_tipo_doc");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.IdUsuarioAuditor).HasColumnName("id_usuario_auditor");
            entity.Property(e => e.PresentadoFisico)
                .HasDefaultValue(false)
                .HasColumnName("presentado_fisico");
            entity.Property(e => e.RutaArchivo)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("ruta_archivo");

            entity.HasOne(d => d.IdTipoDocNavigation).WithMany(p => p.Legajos)
                .HasForeignKey(d => d.IdTipoDoc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Legajo_TipoDoc");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.LegajoIdUsuarioNavigations)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Legajo_Usuario");

            entity.HasOne(d => d.IdUsuarioAuditorNavigation).WithMany(p => p.LegajoIdUsuarioAuditorNavigations)
                .HasForeignKey(d => d.IdUsuarioAuditor)
                .HasConstraintName("FK_Legajo_Auditor");
        });

        modelBuilder.Entity<Materia>(entity =>
        {
            entity.HasKey(e => e.IdMateria).HasName("PK__materias__7E03FD393D134B12");

            entity.ToTable("materias");

            entity.Property(e => e.IdMateria).HasColumnName("id_materia");
            entity.Property(e => e.Carrera)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("carrera");
            entity.Property(e => e.Curso)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("curso");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Pai>(entity =>
        {
            entity.HasKey(e => e.IdPais).HasName("PK__Pais__0941A3A7968A07F9");

            entity.Property(e => e.IdPais).HasColumnName("id_pais");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<PlanesEstudio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PlanesEs__3214EC0789CB904D");

            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PlanesMateria>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PlanesMa__3214EC07DC9FD814");

            entity.HasOne(d => d.Materia).WithMany(p => p.PlanesMateria)
                .HasForeignKey(d => d.MateriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlanMateria_Materia");

            entity.HasOne(d => d.PlanEstudio).WithMany(p => p.PlanesMateria)
                .HasForeignKey(d => d.PlanEstudioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlanMateria_PlanEstudio");
        });

        modelBuilder.Entity<ProgramasMaterium>(entity =>
        {
            entity.HasKey(e => e.IdPrograma).HasName("PK__programa__DC3C5BC16496A1A3");

            entity.ToTable("programas_materia");

            entity.Property(e => e.IdPrograma).HasColumnName("id_programa");
            entity.Property(e => e.CicloLectivo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("ciclo_lectivo");
            entity.Property(e => e.Condicion)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("condicion");
            entity.Property(e => e.CondicionLibre)
                .IsUnicode(false)
                .HasColumnName("condicion_libre");
            entity.Property(e => e.CondicionPromocional)
                .IsUnicode(false)
                .HasColumnName("condicion_promocional");
            entity.Property(e => e.CondicionRegular)
                .IsUnicode(false)
                .HasColumnName("condicion_regular");
            entity.Property(e => e.CriteriosEvaluacion)
                .IsUnicode(false)
                .HasColumnName("criterios_evaluacion");
            entity.Property(e => e.EstrategiasAcompanamientoVirtualRemoto)
                .IsUnicode(false)
                .HasColumnName("estrategias_acompanamiento_virtual_remoto");
            entity.Property(e => e.EstrategiasMetodologicas)
                .IsUnicode(false)
                .HasColumnName("estrategias_metodologicas");
            entity.Property(e => e.Evaluacion)
                .IsUnicode(false)
                .HasColumnName("evaluacion");
            entity.Property(e => e.ExamenesVirtuales)
                .IsUnicode(false)
                .HasColumnName("examenes_virtuales");
            entity.Property(e => e.FormatoCurricular)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("formato_curricular");
            entity.Property(e => e.Fundamentacion)
                .IsUnicode(false)
                .HasColumnName("fundamentacion");
            entity.Property(e => e.HorasCuatrimestrales)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("horas_cuatrimestrales");
            entity.Property(e => e.HorasSemanales)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("horas_semanales");
            entity.Property(e => e.IdDocente).HasColumnName("id_docente");
            entity.Property(e => e.IdMateria).HasColumnName("id_materia");
            entity.Property(e => e.ObjetivosEspecificos)
                .IsUnicode(false)
                .HasColumnName("objetivos_especificos");
            entity.Property(e => e.ObjetivosGenerales)
                .IsUnicode(false)
                .HasColumnName("objetivos_generales");

            entity.HasOne(d => d.IdDocenteNavigation).WithMany(p => p.ProgramasMateria)
                .HasForeignKey(d => d.IdDocente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProgramasMateria_Docente");

            entity.HasOne(d => d.IdMateriaNavigation).WithMany(p => p.ProgramasMateria)
                .HasForeignKey(d => d.IdMateria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProgramasMateria_Materia");
        });

        modelBuilder.Entity<Provincium>(entity =>
        {
            entity.HasKey(e => e.IdProvincia).HasName("PK__Provinci__66C18BFD3E1D283E");

            entity.Property(e => e.IdProvincia).HasColumnName("id_provincia");
            entity.Property(e => e.IdPais).HasColumnName("id_pais");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");

            entity.HasOne(d => d.IdPaisNavigation).WithMany(p => p.Provincia)
                .HasForeignKey(d => d.IdPais)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Provincia_Pais");
        });

        modelBuilder.Entity<ReconocimientoSabere>(entity =>
        {
            entity.HasKey(e => e.IdSolicitud).HasName("PK__reconoci__5C0C31F37B43CC73");

            entity.ToTable("reconocimiento_saberes");

            entity.Property(e => e.IdSolicitud).HasColumnName("id_solicitud");
            entity.Property(e => e.Comentario)
                .IsUnicode(false)
                .HasColumnName("comentario");
            entity.Property(e => e.IdAlumno).HasColumnName("id_alumno");
            entity.Property(e => e.IdDocente).HasColumnName("id_docente");
            entity.Property(e => e.IdMateria).HasColumnName("id_materia");

            entity.HasOne(d => d.IdAlumnoNavigation).WithMany(p => p.ReconocimientoSaberes)
                .HasForeignKey(d => d.IdAlumno)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reconocimiento_Alumno");

            entity.HasOne(d => d.IdDocenteNavigation).WithMany(p => p.ReconocimientoSaberes)
                .HasForeignKey(d => d.IdDocente)
                .HasConstraintName("FK_Reconocimiento_Docente");

            entity.HasOne(d => d.IdMateriaNavigation).WithMany(p => p.ReconocimientoSaberes)
                .HasForeignKey(d => d.IdMateria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reconocimiento_Materia");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__Roles__6ABCB5E0147664D8");

            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.Rol)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("rol");
        });

        modelBuilder.Entity<RolesTiposDocumento>(entity =>
        {
            entity.HasKey(e => e.IdRolesTiposDocumentos).HasName("PK__roles_ti__43644D109721EA86");

            entity.ToTable("roles_tipos_documentos");

            entity.Property(e => e.IdRolesTiposDocumentos).HasColumnName("id_roles_tipos_documentos");
            entity.Property(e => e.Anual).HasColumnName("anual");
            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.IdTipoDoc).HasColumnName("id_tipo_doc");
            entity.Property(e => e.Obligatorio).HasColumnName("obligatorio");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.RolesTiposDocumentos)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RolesTiposDoc_Rol");

            entity.HasOne(d => d.IdTipoDocNavigation).WithMany(p => p.RolesTiposDocumentos)
                .HasForeignKey(d => d.IdTipoDoc)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RolesTiposDoc_TipoDoc");
        });

        modelBuilder.Entity<TipoExaman>(entity =>
        {
            entity.HasKey(e => e.IdTipoExamen).HasName("PK__tipo_exa__C593611A0280B295");

            entity.ToTable("tipo_examen");

            entity.Property(e => e.IdTipoExamen).HasColumnName("id_tipo_examen");
            entity.Property(e => e.TipoExamen)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("tipo_examen");
        });

        modelBuilder.Entity<TipoTitulo>(entity =>
        {
            entity.HasKey(e => e.IdTipoTitulo).HasName("PK__tipo_tit__59FB2E76FF2AAB4B");

            entity.ToTable("tipo_titulo");

            entity.Property(e => e.IdTipoTitulo).HasColumnName("id_tipo_titulo");
            entity.Property(e => e.NombreTitulo)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("nombre_titulo");
        });

        modelBuilder.Entity<TiposDocumento>(entity =>
        {
            entity.HasKey(e => e.IdTipoDoc).HasName("PK__tipos_do__B0A524EAF31C6004");

            entity.ToTable("tipos_documentos");

            entity.Property(e => e.IdTipoDoc).HasColumnName("id_tipo_doc");
            entity.Property(e => e.NombreDocumento)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre_documento");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuarios__4E3E04AD440FDA09");

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.AfiliacionEmergencia)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("afiliacion_emergencia");
            entity.Property(e => e.Apellido)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("apellido");
            entity.Property(e => e.ContactoEmergencia)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("contacto_emergencia");
            entity.Property(e => e.Cuil)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("cuil");
            entity.Property(e => e.Direccion)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("direccion");
            entity.Property(e => e.Dni)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("dni");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.EstadoUsuario)
                .HasDefaultValue(true)
                .HasColumnName("estado_usuario");
            entity.Property(e => e.ExpiracionToken)
                .HasColumnType("datetime")
                .HasColumnName("expiracion_token");
            entity.Property(e => e.FechaNac).HasColumnName("fecha_nac");
            entity.Property(e => e.Genero)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("genero");
            entity.Property(e => e.IdProvincia).HasColumnName("id_provincia");
            entity.Property(e => e.LugarNacimiento)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("lugar_nacimiento");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password_hash");
            entity.Property(e => e.Telefono)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("telefono");
            entity.Property(e => e.TelefonoEmergencia)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("telefono_emergencia");
            entity.Property(e => e.TokenRecuperacion)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("token_recuperacion");

            entity.HasOne(d => d.IdProvinciaNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdProvincia)
                .HasConstraintName("FK_Usuarios_Provincia");
        });

        modelBuilder.Entity<UsuariosRole>(entity =>
        {
            entity.HasKey(e => e.IdUsuarioRol).HasName("PK__Usuarios__D1F881FEEE24C5F9");

            entity.ToTable("Usuarios_roles");

            entity.Property(e => e.IdUsuarioRol).HasColumnName("id_usuario_rol");
            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.UsuariosRoles)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UsuariosRoles_Rol");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.UsuariosRoles)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UsuariosRoles_Usuario");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}