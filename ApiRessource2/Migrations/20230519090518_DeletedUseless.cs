using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiRessource2.Migrations
{
    public partial class DeletedUseless : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportedComments");

            migrationBuilder.DropTable(
                name: "ReportedRessources");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "ZoneGeos",
                newName: "NomCommune");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "ZoneGeos",
                newName: "CodePostale");

            migrationBuilder.RenameColumn(
                name: "RessourceId",
                table: "Voteds",
                newName: "ResourceId");

            migrationBuilder.RenameColumn(
                name: "RessourceId",
                table: "Favoris",
                newName: "ResourceId");

            migrationBuilder.RenameColumn(
                name: "RessourceId",
                table: "Comments",
                newName: "ResourceId");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Voteds",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Voteds_ResourceId",
                table: "Voteds",
                column: "ResourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Resources_UserId",
                table: "Resources",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Favoris_ResourceId",
                table: "Favoris",
                column: "ResourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ResourceId",
                table: "Comments",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Resources_ResourceId",
                table: "Comments",
                column: "ResourceId",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_UserId",
                table: "Comments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Favoris_Resources_ResourceId",
                table: "Favoris",
                column: "ResourceId",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Resources_Users_UserId",
                table: "Resources",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Voteds_Resources_ResourceId",
                table: "Voteds",
                column: "ResourceId",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Resources_ResourceId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Users_UserId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Favoris_Resources_ResourceId",
                table: "Favoris");

            migrationBuilder.DropForeignKey(
                name: "FK_Resources_Users_UserId",
                table: "Resources");

            migrationBuilder.DropForeignKey(
                name: "FK_Voteds_Resources_ResourceId",
                table: "Voteds");

            migrationBuilder.DropIndex(
                name: "IX_Voteds_ResourceId",
                table: "Voteds");

            migrationBuilder.DropIndex(
                name: "IX_Resources_UserId",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Favoris_ResourceId",
                table: "Favoris");

            migrationBuilder.DropIndex(
                name: "IX_Comments_ResourceId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_UserId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Voteds");

            migrationBuilder.RenameColumn(
                name: "NomCommune",
                table: "ZoneGeos",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "CodePostale",
                table: "ZoneGeos",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "ResourceId",
                table: "Voteds",
                newName: "RessourceId");

            migrationBuilder.RenameColumn(
                name: "ResourceId",
                table: "Favoris",
                newName: "RessourceId");

            migrationBuilder.RenameColumn(
                name: "ResourceId",
                table: "Comments",
                newName: "RessourceId");

            migrationBuilder.CreateTable(
                name: "ReportedComments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CommentId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsApprouved = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Reason = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportedComments", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ReportedRessources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsApprouved = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Reason = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RessourceId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportedRessources", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
