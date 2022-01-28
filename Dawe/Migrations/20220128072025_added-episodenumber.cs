using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dawe.Migrations
{
    public partial class addedepisodenumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "episodeNumber",
                table: "Episodes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "episodeNumber",
                table: "Episodes");
        }
    }
}
