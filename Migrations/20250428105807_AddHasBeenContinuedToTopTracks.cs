using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mazzika.Migrations
{
    /// <inheritdoc />
    public partial class AddHasBeenContinuedToTopTracks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TopTracks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VideoId = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "TEXT", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ChannelTitle = table.Column<string>(type: "TEXT", nullable: false),
                    PlayCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LastPlayed = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    HasBeenContinued = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopTracks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TopTracks_LastPlayed",
                table: "TopTracks",
                column: "LastPlayed");

            migrationBuilder.CreateIndex(
                name: "IX_TopTracks_PlayCount",
                table: "TopTracks",
                column: "PlayCount");

            migrationBuilder.CreateIndex(
                name: "IX_TopTracks_VideoId",
                table: "TopTracks",
                column: "VideoId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TopTracks");
        }
    }
}
