using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aluguer_Salas.Migrations
{
    /// <inheritdoc />
    public partial class SalaDisponivel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Disponibilidades_Salas_SalaId",
                table: "Disponibilidades");

            migrationBuilder.DropForeignKey(
                name: "FK_Limpeza_Funcionario_FuncionarioId",
                table: "Limpeza");

            migrationBuilder.DropForeignKey(
                name: "FK_Limpeza_Salas_SalaId",
                table: "Limpeza");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_Salas_IdSala",
                table: "Reservas");

            migrationBuilder.DropIndex(
                name: "IX_Reservas_IdSala",
                table: "Reservas");

            migrationBuilder.DropIndex(
                name: "IX_Limpeza_FuncionarioId",
                table: "Limpeza");

            migrationBuilder.DropIndex(
                name: "IX_Limpeza_SalaId",
                table: "Limpeza");

            migrationBuilder.DropIndex(
                name: "IX_Disponibilidades_SalaId",
                table: "Disponibilidades");

            migrationBuilder.DropColumn(
                name: "FuncionarioId",
                table: "Limpeza");

            migrationBuilder.DropColumn(
                name: "SalaId",
                table: "Limpeza");

            migrationBuilder.DropColumn(
                name: "SalaId",
                table: "Disponibilidades");

            migrationBuilder.AddColumn<bool>(
                name: "Disponivel",
                table: "Salas",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SalaId",
                table: "Reservas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_SalaId",
                table: "Reservas",
                column: "SalaId");

            migrationBuilder.CreateIndex(
                name: "IX_Limpeza_IdUtilizador",
                table: "Limpeza",
                column: "IdUtilizador");

            migrationBuilder.CreateIndex(
                name: "IX_Disponibilidades_IdSala",
                table: "Disponibilidades",
                column: "IdSala");

            migrationBuilder.AddForeignKey(
                name: "FK_Disponibilidades_Salas_IdSala",
                table: "Disponibilidades",
                column: "IdSala",
                principalTable: "Salas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Limpeza_Funcionario_IdUtilizador",
                table: "Limpeza",
                column: "IdUtilizador",
                principalTable: "Funcionario",
                principalColumn: "FuncionarioId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Limpeza_Salas_IdSala",
                table: "Limpeza",
                column: "IdSala",
                principalTable: "Salas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_Salas_SalaId",
                table: "Reservas",
                column: "SalaId",
                principalTable: "Salas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Disponibilidades_Salas_IdSala",
                table: "Disponibilidades");

            migrationBuilder.DropForeignKey(
                name: "FK_Limpeza_Funcionario_IdUtilizador",
                table: "Limpeza");

            migrationBuilder.DropForeignKey(
                name: "FK_Limpeza_Salas_IdSala",
                table: "Limpeza");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_Salas_SalaId",
                table: "Reservas");

            migrationBuilder.DropIndex(
                name: "IX_Reservas_SalaId",
                table: "Reservas");

            migrationBuilder.DropIndex(
                name: "IX_Limpeza_IdUtilizador",
                table: "Limpeza");

            migrationBuilder.DropIndex(
                name: "IX_Disponibilidades_IdSala",
                table: "Disponibilidades");

            migrationBuilder.DropColumn(
                name: "Disponivel",
                table: "Salas");

            migrationBuilder.DropColumn(
                name: "SalaId",
                table: "Reservas");

            migrationBuilder.AddColumn<int>(
                name: "FuncionarioId",
                table: "Limpeza",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SalaId",
                table: "Limpeza",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SalaId",
                table: "Disponibilidades",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_IdSala",
                table: "Reservas",
                column: "IdSala");

            migrationBuilder.CreateIndex(
                name: "IX_Limpeza_FuncionarioId",
                table: "Limpeza",
                column: "FuncionarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Limpeza_SalaId",
                table: "Limpeza",
                column: "SalaId");

            migrationBuilder.CreateIndex(
                name: "IX_Disponibilidades_SalaId",
                table: "Disponibilidades",
                column: "SalaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Disponibilidades_Salas_SalaId",
                table: "Disponibilidades",
                column: "SalaId",
                principalTable: "Salas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Limpeza_Funcionario_FuncionarioId",
                table: "Limpeza",
                column: "FuncionarioId",
                principalTable: "Funcionario",
                principalColumn: "FuncionarioId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Limpeza_Salas_SalaId",
                table: "Limpeza",
                column: "SalaId",
                principalTable: "Salas",
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
    }
}
