using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoGestionAPI.Migrations
{
    /// <inheritdoc />
    public partial class ExamenDocenteCreador : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "id_docente_creador",
                table: "Examenes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Examenes_id_docente_creador",
                table: "Examenes",
                column: "id_docente_creador");

            migrationBuilder.AddForeignKey(
                name: "FK_Examenes_DocenteCreador",
                table: "Examenes",
                column: "id_docente_creador",
                principalTable: "Docentes",
                principalColumn: "id_docente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Examenes_DocenteCreador",
                table: "Examenes");

            migrationBuilder.DropIndex(
                name: "IX_Examenes_id_docente_creador",
                table: "Examenes");

            migrationBuilder.DropColumn(
                name: "id_docente_creador",
                table: "Examenes");
        }
    }
}
