using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BanningApplications.WebApi.Migrations.UnsplashDb
{
    public partial class Unsplash_Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UnsplashPhotographers",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 36, nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: false),
                    Archived = table.Column<bool>(nullable: false),
                    ModifiedBy = table.Column<string>(maxLength: 250, nullable: false),
                    UserName = table.Column<string>(maxLength: 50, nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Location = table.Column<string>(maxLength: 100, nullable: true),
                    Bio = table.Column<string>(maxLength: 500, nullable: true),
                    Portfolio = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnsplashPhotographers", x => x.Id);
                    table.UniqueConstraint("AK_UnsplashPhotographers_UserName", x => x.UserName);
                });

            migrationBuilder.CreateTable(
                name: "UnsplashPhotos",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 36, nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: false),
                    Archived = table.Column<bool>(nullable: false),
                    ModifiedBy = table.Column<string>(maxLength: 250, nullable: false),
                    UserName = table.Column<string>(maxLength: 50, nullable: false),
                    Width = table.Column<int>(nullable: false),
                    Height = table.Column<int>(nullable: false),
                    Published = table.Column<DateTime>(nullable: false),
                    BlurHash = table.Column<string>(maxLength: 50, nullable: false),
                    Description = table.Column<string>(maxLength: 100, nullable: true),
                    AltDescription = table.Column<string>(maxLength: 100, nullable: true),
                    Color = table.Column<string>(maxLength: 20, nullable: true),
                    Location = table.Column<string>(maxLength: 100, nullable: true),
                    TagsJson = table.Column<string>(nullable: true),
                    TopicsJson = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnsplashPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnsplashPhotos_UnsplashPhotographers_UserName",
                        column: x => x.UserName,
                        principalTable: "UnsplashPhotographers",
                        principalColumn: "UserName",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UnsplashAwardWinners",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 36, nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    ModifyDate = table.Column<DateTime>(nullable: false),
                    Archived = table.Column<bool>(nullable: false),
                    ModifiedBy = table.Column<string>(maxLength: 250, nullable: false),
                    PhotoId = table.Column<string>(maxLength: 36, nullable: false),
                    Year = table.Column<int>(nullable: false),
                    Category = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnsplashAwardWinners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnsplashAwardWinners_UnsplashPhotos_PhotoId",
                        column: x => x.PhotoId,
                        principalTable: "UnsplashPhotos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UnsplashAwardWinners_PhotoId",
                table: "UnsplashAwardWinners",
                column: "PhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_UnsplashPhotos_UserName",
                table: "UnsplashPhotos",
                column: "UserName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UnsplashAwardWinners");

            migrationBuilder.DropTable(
                name: "UnsplashPhotos");

            migrationBuilder.DropTable(
                name: "UnsplashPhotographers");
        }
    }
}
