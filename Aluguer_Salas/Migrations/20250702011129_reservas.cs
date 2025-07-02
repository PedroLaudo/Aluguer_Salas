using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aluguer_Salas.Migrations
{
    /// <inheritdoc />
    public partial class reservas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Limpezas");

            migrationBuilder.DropTable(
                name: "Funcionarios");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Funcionarios",
                columns: table => new
                {
                    FuncionarioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UtilizadorId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funcionarios", x => x.FuncionarioId);
                    table.ForeignKey(
                        name: "FK_Funcionarios_AspNetUsers_UtilizadorId",
                        column: x => x.UtilizadorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Limpezas",
                columns: table => new
                {
                    IdSala = table.Column<int>(type: "int", nullable: false),
                    IdUtilizador = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Limpezas", x => new { x.IdSala, x.IdUtilizador });
                    table.ForeignKey(
                        name: "FK_Limpezas_Funcionarios_IdUtilizador",
                        column: x => x.IdUtilizador,
                        principalTable: "Funcionarios",
                        principalColumn: "FuncionarioId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Limpezas_Salas_IdSala",
                        column: x => x.IdSala,
                        principalTable: "Salas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Funcionarios_UtilizadorId",
                table: "Funcionarios",
                column: "UtilizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Limpezas_IdUtilizador",
                table: "Limpezas",
                column: "IdUtilizador");
        }
    }
}
