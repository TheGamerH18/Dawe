using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dawe.Migrations
{
    public partial class deletecoverfilefromepisode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cover",
                table: "Episodes");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Cover",
                table: "Episodes",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
