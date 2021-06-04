using Microsoft.EntityFrameworkCore.Migrations;

namespace SlickTicket.data.migration
{
    public partial class _004 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "Ticket");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "Ticket",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
