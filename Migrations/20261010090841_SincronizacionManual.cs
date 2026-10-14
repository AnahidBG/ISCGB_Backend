using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoGestionAPI.Migrations
{
    /// <inheritdoc />
    public partial class SincronizacionManual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'estado_confirmacion' AND Object_ID = Object_ID(N'dbo.mesa_examen'))
        BEGIN
            ALTER TABLE mesa_examen ADD estado_confirmacion VARCHAR(20) DEFAULT 'Pendiente';
        END
    ");
            migrationBuilder.Sql(@"
        IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'limite_parciales_semana' AND Object_ID = Object_ID(N'dbo.configuracion_sistema'))
        BEGIN
            EXEC sp_rename 'configuracion_sistema.limite_parciales_semana', 'limite_parciales_diario', 'COLUMN';
        END
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
