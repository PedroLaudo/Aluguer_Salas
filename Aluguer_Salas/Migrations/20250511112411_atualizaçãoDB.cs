using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aluguer_Salas.Migrations
{
    /// <inheritdoc />
    public partial class atualizaçãoDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Funcionario_AspNetUsers_UtilizadorId",
                table: "Funcionario");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_Salas_SalaId",
                table: "Reservas");

            migrationBuilder.DropTable(
                name: "Disponibilidades");

            migrationBuilder.DropTable(
                name: "Limpeza");

            migrationBuilder.DropIndex(
                name: "IX_Reservas_SalaId",
                table: "Reservas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Funcionario",
                table: "Funcionario");

            migrationBuilder.DropColumn(
                name: "SalaId",
                table: "Reservas");

            migrationBuilder.RenameTable(
                name: "Funcionario",
                newName: "Funcionarios");

            migrationBuilder.RenameIndex(
                name: "IX_Funcionario_UtilizadorId",
                table: "Funcionarios",
                newName: "IX_Funcionarios_UtilizadorId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Funcionarios",
                table: "Funcionarios",
                column: "FuncionarioId");

            migrationBuilder.CreateTable(
                name: "Limpezas",
                columns: table => new
                {
                    LimpezaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSala = table.Column<int>(type: "int", nullable: false),
                    FuncionarioId = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Limpezas", x => x.LimpezaId);
                    table.ForeignKey(
                        name: "FK_Limpezas_Funcionarios_FuncionarioId",
                        column: x => x.FuncionarioId,
                        principalTable: "Funcionarios",
                        principalColumn: "FuncionarioId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Limpezas_Salas_IdSala",
                        column: x => x.IdSala,
                        principalTable: "Salas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_IdSala",
                table: "Reservas",
                column: "IdSala");

            migrationBuilder.CreateIndex(
                name: "IX_Limpezas_FuncionarioId",
                table: "Limpezas",
                column: "FuncionarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Limpezas_IdSala",
                table: "Limpezas",
                column: "IdSala");

            migrationBuilder.AddForeignKey(
                name: "FK_Funcionarios_AspNetUsers_UtilizadorId",
                table: "Funcionarios",
                column: "UtilizadorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_Salas_IdSala",
                table: "Reservas",
                column: "IdSala",
                principalTable: "Salas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Funcionarios_AspNetUsers_UtilizadorId",
                table: "Funcionarios");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_Salas_IdSala",
                table: "Reservas");

            migrationBuilder.DropTable(
                name: "Limpezas");

            migrationBuilder.DropIndex(
                name: "IX_Reservas_IdSala",
                table: "Reservas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Funcionarios",
                table: "Funcionarios");

            migrationBuilder.RenameTable(
                name: "Funcionarios",
                newName: "Funcionario");

            migrationBuilder.RenameIndex(
                name: "IX_Funcionarios_UtilizadorId",
                table: "Funcionario",
                newName: "IX_Funcionario_UtilizadorId");

            migrationBuilder.AddColumn<int>(
                name: "SalaId",
                table: "Reservas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Funcionario",
                table: "Funcionario",
                column: "FuncionarioId");

            migrationBuilder.CreateTable(
                name: "Disponibilidades",
                columns: table => new
                {
                    IdDisponibilidade = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSala = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HoraFim = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disponibilidades", x => x.IdDisponibilidade);
                    table.ForeignKey(
                        name: "FK_Disponibilidades_Salas_IdSala",
                        column: x => x.IdSala,
                        principalTable: "Salas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Limpeza",
                columns: table => new
                {
                    IdSala = table.Column<int>(type: "int", nullable: false),
                    IdUtilizador = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Limpeza", x => new { x.IdSala, x.IdUtilizador });
                    table.ForeignKey(
                        name: "FK_Limpeza_Funcionario_IdUtilizador",
                        column: x => x.IdUtilizador,
                        principalTable: "Funcionario",
                        principalColumn: "FuncionarioId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Limpeza_Salas_IdSala",
                        column: x => x.IdSala,
                        principalTable: "Salas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_SalaId",
                table: "Reservas",
                column: "SalaId");

            migrationBuilder.CreateIndex(
                name: "IX_Disponibilidades_IdSala",
                table: "Disponibilidades",
                column: "IdSala");

            migrationBuilder.CreateIndex(
                name: "IX_Limpeza_IdUtilizador",
                table: "Limpeza",
                column: "IdUtilizador");

            migrationBuilder.AddForeignKey(
                name: "FK_Funcionario_AspNetUsers_UtilizadorId",
                table: "Funcionario",
                column: "UtilizadorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_Salas_SalaId",
                table: "Reservas",
                column: "SalaId",
                principalTable: "Salas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
