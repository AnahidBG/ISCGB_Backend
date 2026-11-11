using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoGestionAPI.Migrations
{
    /// <summary>
    /// Las mesas de examen pasan a ser por materia (y año) para todas las comisiones:
    /// se guardan sin comisión y con el tipo "Mesa de Examen". Las mesas que ya
    /// estaban cargadas (con comisión y tipo Final) se pasan al formato nuevo.
    /// </summary>
    public partial class MesaExamenSinComision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "id_comision",
                table: "Examenes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            // El tipo nuevo se busca por nombre (el id puede variar entre bases)
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM tipo_examen WHERE tipo_examen = 'Mesa de Examen')
    INSERT INTO tipo_examen (tipo_examen) VALUES ('Mesa de Examen');");

            // Las mesas existentes (exámenes con tribunal) quedan sin comisión
            migrationBuilder.Sql(@"
UPDATE Examenes
SET id_comision = NULL,
    id_tipo_examen = (SELECT TOP 1 id_tipo_examen FROM tipo_examen WHERE tipo_examen = 'Mesa de Examen')
WHERE id_examen IN (SELECT id_examen FROM mesa_examen);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Para volver a la columna obligatoria, las mesas toman la primera
            // comisión y el tipo Final, como estaban antes
            migrationBuilder.Sql(@"
UPDATE Examenes
SET id_comision = (SELECT MIN(id_comision) FROM comision),
    id_tipo_examen = COALESCE((SELECT TOP 1 id_tipo_examen FROM tipo_examen WHERE tipo_examen = 'Final'), id_tipo_examen)
WHERE id_comision IS NULL;");

            migrationBuilder.AlterColumn<int>(
                name: "id_comision",
                table: "Examenes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
