using Microsoft.EntityFrameworkCore.Migrations;

namespace BanningApplications.WebApi.Migrations.UnsplashDb
{
    public partial class UnsplashAwardAddWinner : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Winner",
                table: "UnsplashAwardWinners",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Winner",
                table: "UnsplashAwardWinners");
        }
    }
}
