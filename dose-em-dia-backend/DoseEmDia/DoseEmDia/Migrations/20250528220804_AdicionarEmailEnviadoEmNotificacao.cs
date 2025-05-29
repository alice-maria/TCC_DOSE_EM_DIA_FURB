using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoseEmDia.Migrations
{
    public partial class AdicionarEmailEnviadoEmNotificacao : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailEnviado",
                table: "Notificacao",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailEnviado",
                table: "Notificacao");
        }
    }
}
