using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aluguer_Salas.Migrations
{
    /// <inheritdoc />
    public partial class Inicial2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalaId",
                table: "Limpeza",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Limpeza_SalaId",
                table: "Limpeza",
                column: "SalaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Limpeza_Salas_SalaId",
                table: "Limpeza",
                column: "SalaId",
                principalTable: "Salas",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Limpeza_Salas_SalaId",
                table: "Limpeza");

            migrationBuilder.DropIndex(
                name: "IX_Limpeza_SalaId",
                table: "Limpeza");

            migrationBuilder.DropColumn(
                name: "SalaId",
                table: "Limpeza");
        }
    }
}
