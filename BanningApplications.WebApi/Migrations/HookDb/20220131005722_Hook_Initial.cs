using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BanningApplications.WebApi.Migrations.HookDb
{
    public partial class Hook_Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HookRequests",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 100, nullable: false),
                    Path = table.Column<string>(nullable: true),
                    Headers = table.Column<string>(nullable: true),
                    Body = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HookRequests", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HookRequests");
        }
    }
}
