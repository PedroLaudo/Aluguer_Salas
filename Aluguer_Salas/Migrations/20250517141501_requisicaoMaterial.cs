using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aluguer_Salas.Migrations
{
    /// <inheritdoc />
    public partial class requisicaoMaterial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Materiais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QuantidadeDisponivel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materiais", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RequisicoesMaterial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UtilizadorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaterialId = table.Column<int>(type: "int", nullable: false),
                    DataRequisicao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraFim = table.Column<TimeSpan>(type: "time", nullable: false),
                    QuantidadeRequisitada = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequisicoesMaterial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequisicoesMaterial_AspNetUsers_UtilizadorId",
                        column: x => x.UtilizadorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequisicoesMaterial_Materiais_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materiais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequisicoesMaterial_MaterialId",
                table: "RequisicoesMaterial",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_RequisicoesMaterial_UtilizadorId",
                table: "RequisicoesMaterial",
                column: "UtilizadorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequisicoesMaterial");

            migrationBuilder.DropTable(
                name: "Materiais");
        }
    }
}
