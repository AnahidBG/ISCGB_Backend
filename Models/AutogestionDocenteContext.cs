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

    public virtual DbSet<Correlatividade> Correlatividades { get; set; }

    public virtual DbSet<PlanesEstudio> PlanesEstudios { get; set; }

    public virtual DbSet<PlanesMateria> PlanesMaterias { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost\\SQLEXPRESS;Database=Autogestion_Docente;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Correlatividade>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Correlat__3214EC07CAE8510A");

            entity.HasOne(d => d.PlanEstudio).WithMany(p => p.Correlatividades)
                .HasForeignKey(d => d.PlanEstudioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Correlatividad_PlanEstudio");
        });

        modelBuilder.Entity<PlanesEstudio>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PlanesEs__3214EC07A70B4724");

            entity.Property(e => e.Estado)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PlanesMateria>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PlanesMa__3214EC073254077F");

            entity.HasOne(d => d.PlanEstudio).WithMany(p => p.PlanesMateria)
                .HasForeignKey(d => d.PlanEstudioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlanMateria_PlanEstudio");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
