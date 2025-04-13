using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoseEmDia.Migrations
{
    public partial class AddDataNascimentoToUsuario : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataNascimento",
                table: "Usuario",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime());
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataNascimento",
                table: "Usuario");
        }
    }
}
