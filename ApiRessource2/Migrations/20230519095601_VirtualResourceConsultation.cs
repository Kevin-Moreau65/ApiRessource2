using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiRessource2.Migrations
{
    public partial class VirtualResourceConsultation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RessourceId",
                table: "Consultations",
                newName: "ResourceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResourceId",
                table: "Consultations",
                newName: "RessourceId");
        }
    }
}
