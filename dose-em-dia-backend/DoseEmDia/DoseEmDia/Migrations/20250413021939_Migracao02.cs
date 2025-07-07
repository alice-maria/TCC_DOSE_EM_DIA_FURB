using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DoseEmDia.Migrations
{
    public partial class Migracao02 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataNascimento",
                table: "Usuario",
                nullable: false,
                defaultValue: new DateTime(1900, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "TokenRedefinicaoSenha",
                table: "Usuario",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataNascimento",
                table: "Usuario");

            migrationBuilder.DropColumn(
                name: "TokenRedefinicaoSenha",
                table: "Usuario");
        }
    }
}
