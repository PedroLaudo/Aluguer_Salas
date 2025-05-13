using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aluguer_Salas.Migrations
{
    /// <inheritdoc />
    public partial class CorrigirRelacionamentosReservas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_AspNetUsers_UtilizadorId",
                table: "Reservas");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_Salas_SalaId",
                table: "Reservas");

            migrationBuilder.DropIndex(
                name: "IX_Reservas_SalaId",
                table: "Reservas");

            migrationBuilder.DropIndex(
                name: "IX_Reservas_UtilizadorId",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "SalaId",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "UtilizadorId",
                table: "Reservas");

            migrationBuilder.RenameColumn(
                name: "IdUtilizador",
                table: "Reservas",
                newName: "IdSala");

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Salas",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "UtilizadorIdentityId",
                table: "Reservas",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_IdSala",
                table: "Reservas",
                column: "IdSala");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_UtilizadorIdentityId",
                table: "Reservas",
                column: "UtilizadorIdentityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_AspNetUsers_UtilizadorIdentityId",
                table: "Reservas",
                column: "UtilizadorIdentityId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_Salas_IdSala",
                table: "Reservas",
                column: "IdSala",
                principalTable: "Salas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_AspNetUsers_UtilizadorIdentityId",
                table: "Reservas");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_Salas_IdSala",
                table: "Reservas");

            migrationBuilder.DropIndex(
                name: "IX_Reservas_IdSala",
                table: "Reservas");

            migrationBuilder.DropIndex(
                name: "IX_Reservas_UtilizadorIdentityId",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "UtilizadorIdentityId",
                table: "Reservas");

            migrationBuilder.RenameColumn(
                name: "IdSala",
                table: "Reservas",
                newName: "IdUtilizador");

            migrationBuilder.AlterColumn<string>(
                name: "Descricao",
                table: "Salas",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalaId",
                table: "Reservas",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UtilizadorId",
                table: "Reservas",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_SalaId",
                table: "Reservas",
                column: "SalaId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_UtilizadorId",
                table: "Reservas",
                column: "UtilizadorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_AspNetUsers_UtilizadorId",
                table: "Reservas",
                column: "UtilizadorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_Salas_SalaId",
                table: "Reservas",
                column: "SalaId",
                principalTable: "Salas",
                principalColumn: "Id");
        }
    }
}
