using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aluguer_Salas.Migrations
{
    /// <inheritdoc />
    public partial class ajusteUtentes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Utentes_AspNetUsers_UtilizadorId",
                table: "Utentes");

            migrationBuilder.RenameColumn(
                name: "UtilizadorId",
                table: "Utentes",
                newName: "UtilizadorIdentityId");

            migrationBuilder.RenameIndex(
                name: "IX_Utentes_UtilizadorId",
                table: "Utentes",
                newName: "IX_Utentes_UtilizadorIdentityId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Utentes_AspNetUsers_UtilizadorIdentityId",
                table: "Utentes",
                column: "UtilizadorIdentityId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Utentes_AspNetUsers_UtilizadorIdentityId",
                table: "Utentes");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Utentes");

            migrationBuilder.RenameColumn(
                name: "UtilizadorIdentityId",
                table: "Utentes",
                newName: "UtilizadorId");

            migrationBuilder.RenameIndex(
                name: "IX_Utentes_UtilizadorIdentityId",
                table: "Utentes",
                newName: "IX_Utentes_UtilizadorId");

            migrationBuilder.AlterColumn<string>(
                name: "Tipo",
                table: "Utentes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_Utentes_AspNetUsers_UtilizadorId",
                table: "Utentes",
                column: "UtilizadorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
