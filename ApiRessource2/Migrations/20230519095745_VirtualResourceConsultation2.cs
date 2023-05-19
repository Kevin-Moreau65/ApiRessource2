using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiRessource2.Migrations
{
    public partial class VirtualResourceConsultation2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Consultations_ResourceId",
                table: "Consultations",
                column: "ResourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_Resources_ResourceId",
                table: "Consultations",
                column: "ResourceId",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consultations_Resources_ResourceId",
                table: "Consultations");

            migrationBuilder.DropIndex(
                name: "IX_Consultations_ResourceId",
                table: "Consultations");
        }
    }
}
