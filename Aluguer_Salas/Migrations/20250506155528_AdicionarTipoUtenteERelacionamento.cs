using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aluguer_Salas.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTipoUtenteERelacionamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Utentes_UtilizadorId",
                table: "Utentes");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Utentes");

            migrationBuilder.RenameColumn(
                name: "IdUtilizador",
                table: "Utentes",
                newName: "Id");

            migrationBuilder.AlterColumn<string>(
                name: "Tipo",
                table: "Utentes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Utentes_UtilizadorId",
                table: "Utentes",
                column: "UtilizadorId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Utentes_UtilizadorId",
                table: "Utentes");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Utentes",
                newName: "IdUtilizador");

            migrationBuilder.AlterColumn<string>(
                name: "Tipo",
                table: "Utentes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Utentes",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Utentes_UtilizadorId",
                table: "Utentes",
                column: "UtilizadorId");
        }
    }
}
