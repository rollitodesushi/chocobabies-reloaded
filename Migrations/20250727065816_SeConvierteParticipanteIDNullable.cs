using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChocobabiesReloaded.Migrations
{
    /// <inheritdoc />
    public partial class SeConvierteParticipanteIDNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tiquetes_participantes_participanteId",
                table: "tiquetes");

            migrationBuilder.AlterColumn<int>(
                name: "participanteId",
                table: "tiquetes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_tiquetes_participantes_participanteId",
                table: "tiquetes",
                column: "participanteId",
                principalTable: "participantes",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tiquetes_participantes_participanteId",
                table: "tiquetes");

            migrationBuilder.AlterColumn<int>(
                name: "participanteId",
                table: "tiquetes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tiquetes_participantes_participanteId",
                table: "tiquetes",
                column: "participanteId",
                principalTable: "participantes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
