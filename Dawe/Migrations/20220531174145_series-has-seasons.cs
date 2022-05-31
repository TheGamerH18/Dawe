using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dawe.Migrations
{
    public partial class serieshasseasons : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Episodes_Series_showId",
                table: "Episodes");

            migrationBuilder.RenameColumn(
                name: "showId",
                table: "Episodes",
                newName: "seasonId");

            migrationBuilder.RenameIndex(
                name: "IX_Episodes_showId",
                table: "Episodes",
                newName: "IX_Episodes_seasonId");

            migrationBuilder.CreateTable(
                name: "Seasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SeriesId = table.Column<int>(type: "INTEGER", nullable: false),
                    Seasonnumber = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seasons_Series_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "Series",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Seasons_SeriesId",
                table: "Seasons",
                column: "SeriesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Episodes_Seasons_seasonId",
                table: "Episodes",
                column: "seasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Episodes_Seasons_seasonId",
                table: "Episodes");

            migrationBuilder.DropTable(
                name: "Seasons");

            migrationBuilder.RenameColumn(
                name: "seasonId",
                table: "Episodes",
                newName: "showId");

            migrationBuilder.RenameIndex(
                name: "IX_Episodes_seasonId",
                table: "Episodes",
                newName: "IX_Episodes_showId");

            migrationBuilder.AddForeignKey(
                name: "FK_Episodes_Series_showId",
                table: "Episodes",
                column: "showId",
                principalTable: "Series",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
