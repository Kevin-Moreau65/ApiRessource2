using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiRessource2.Migrations
{
    public partial class VirtualUserVoted : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Voteds_UserId",
                table: "Voteds",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Voteds_Users_UserId",
                table: "Voteds",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Voteds_Users_UserId",
                table: "Voteds");

            migrationBuilder.DropIndex(
                name: "IX_Voteds_UserId",
                table: "Voteds");
        }
    }
}
