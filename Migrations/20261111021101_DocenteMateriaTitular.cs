using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoGestionAPI.Migrations
{
    /// <inheritdoc />
    public partial class DocenteMateriaTitular : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "id_docente_titular",
                table: "docente_materia",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_docente_materia_id_docente_titular",
                table: "docente_materia",
                column: "id_docente_titular");

            migrationBuilder.AddForeignKey(
                name: "FK_DocenteMateria_Titular",
                table: "docente_materia",
                column: "id_docente_titular",
                principalTable: "Docentes",
                principalColumn: "id_docente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocenteMateria_Titular",
                table: "docente_materia");

            migrationBuilder.DropIndex(
                name: "IX_docente_materia_id_docente_titular",
                table: "docente_materia");

            migrationBuilder.DropColumn(
                name: "id_docente_titular",
                table: "docente_materia");
        }
    }
}
