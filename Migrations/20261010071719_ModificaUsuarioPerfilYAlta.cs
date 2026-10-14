using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoGestionAPI.Migrations
{
    /// <inheritdoc />
    public partial class ModificaUsuarioPerfilYAlta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "lugar_nacimiento",
                table: "Usuarios",
                newName: "LugarNacimiento");

            migrationBuilder.AlterColumn<string>(
                name: "LugarNacimiento",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldUnicode: false,
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_alta",
                table: "Usuarios",
                type: "datetime",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<string>(
                name: "foto_perfil",
                table: "Usuarios",
                type: "varchar(255)",
                unicode: false,
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fecha_alta",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "foto_perfil",
                table: "Usuarios");

            migrationBuilder.RenameColumn(
                name: "LugarNacimiento",
                table: "Usuarios",
                newName: "lugar_nacimiento");

            migrationBuilder.AlterColumn<string>(
                name: "lugar_nacimiento",
                table: "Usuarios",
                type: "varchar(150)",
                unicode: false,
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
