using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoseEmDia.Migrations
{
    public partial class CorrigirUsuarioId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ReceberNotificacoes",
                table: "Usuario",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceberNotificacoes",
                table: "Usuario");
        }
    }
}
