using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChocobabiesReloaded.Migrations
{
    /// <inheritdoc />
    public partial class SeCreanSorteos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "precioTiquete",
                table: "rifas",
                newName: "precioPorNumero");

            migrationBuilder.RenameColumn(
                name: "nombre",
                table: "rifas",
                newName: "nombreSorteo");

            migrationBuilder.RenameColumn(
                name: "fechaInicioSorteo",
                table: "rifas",
                newName: "fechaCreacion");

            migrationBuilder.AddColumn<bool>(
                name: "estaComprado",
                table: "tiquetes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "cantidadNumeros",
                table: "rifas",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "estaComprado",
                table: "tiquetes");

            migrationBuilder.RenameColumn(
                name: "precioPorNumero",
                table: "rifas",
                newName: "precioTiquete");

            migrationBuilder.RenameColumn(
                name: "nombreSorteo",
                table: "rifas",
                newName: "nombre");

            migrationBuilder.RenameColumn(
                name: "fechaCreacion",
                table: "rifas",
                newName: "fechaInicioSorteo");

            migrationBuilder.AlterColumn<int>(
                name: "cantidadNumeros",
                table: "rifas",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
