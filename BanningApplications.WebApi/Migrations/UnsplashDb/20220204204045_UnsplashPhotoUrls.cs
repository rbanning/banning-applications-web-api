using Microsoft.EntityFrameworkCore.Migrations;

namespace BanningApplications.WebApi.Migrations.UnsplashDb
{
    public partial class UnsplashPhotoUrls : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "BlurHash",
                table: "UnsplashPhotos",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "UrlsJson",
                table: "UnsplashPhotos",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UrlsJson",
                table: "UnsplashPhotos");

            migrationBuilder.AlterColumn<string>(
                name: "BlurHash",
                table: "UnsplashPhotos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
