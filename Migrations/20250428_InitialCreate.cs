using Microsoft.EntityFrameworkCore.Migrations;

namespace Mazzika.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TopTracks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VideoId = table.Column<string>(maxLength: 100, nullable: false),
                    Title = table.Column<string>(maxLength: 200, nullable: false),
                    Description = table.Column<string>(maxLength: 1000, nullable: true),
                    ThumbnailUrl = table.Column<string>(maxLength: 500, nullable: false),
                    PublishedAt = table.Column<DateTime>(nullable: false),
                    ChannelTitle = table.Column<string>(maxLength: 100, nullable: false),
                    PlayCount = table.Column<int>(nullable: false, defaultValue: 0),
                    LastPlayed = table.Column<DateTime>(nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopTracks", x => x.Id);
                    table.UniqueConstraint("UK_TopTracks_VideoId", x => x.VideoId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TopTracks_PlayCount",
                table: "TopTracks",
                column: "PlayCount");

            migrationBuilder.CreateIndex(
                name: "IX_TopTracks_LastPlayed",
                table: "TopTracks",
                column: "LastPlayed");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TopTracks");
        }
    }
}