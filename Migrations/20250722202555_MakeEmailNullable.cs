using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChocobabiesReloaded.Migrations
{
    /// <inheritdoc />
    public partial class MakeEmailNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "cantidadNumeros",
                table: "rifas",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "participantes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cantidadNumeros",
                table: "rifas");

            migrationBuilder.AlterColumn<string>(
                name: "email",
                table: "participantes",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
