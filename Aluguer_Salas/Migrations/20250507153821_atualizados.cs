using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aluguer_Salas.Migrations
{
    /// <inheritdoc />
    public partial class atualizados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdSala",
                table: "Reservas");

            migrationBuilder.RenameColumn(
                name: "DataHoraInicio",
                table: "Reservas",
                newName: "HoraInicio");

            migrationBuilder.RenameColumn(
                name: "DataHoraFim",
                table: "Reservas",
                newName: "HoraFim");

            migrationBuilder.AddColumn<DateTime>(
                name: "Data",
                table: "Reservas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data",
                table: "Reservas");

            migrationBuilder.RenameColumn(
                name: "HoraInicio",
                table: "Reservas",
                newName: "DataHoraInicio");

            migrationBuilder.RenameColumn(
                name: "HoraFim",
                table: "Reservas",
                newName: "DataHoraFim");

            migrationBuilder.AddColumn<int>(
                name: "IdSala",
                table: "Reservas",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
