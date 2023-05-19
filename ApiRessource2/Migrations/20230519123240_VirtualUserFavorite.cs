using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiRessource2.Migrations
{
    public partial class VirtualUserFavorite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Favoris_UserId",
                table: "Favoris",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Favoris_Users_UserId",
                table: "Favoris",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favoris_Users_UserId",
                table: "Favoris");

            migrationBuilder.DropIndex(
                name: "IX_Favoris_UserId",
                table: "Favoris");
        }
    }
}
