using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zetflix.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEpisodeVideoSeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Episodes",
                keyColumn: "Id",
                keyValue: 1,
                column: "VideoUrl",
                value: "/assets/images/video/sample-video.mp4");

            migrationBuilder.UpdateData(
                table: "Episodes",
                keyColumn: "Id",
                keyValue: 2,
                column: "VideoUrl",
                value: "/assets/images/video/trailer.mp4");

            migrationBuilder.UpdateData(
                table: "Episodes",
                keyColumn: "Id",
                keyValue: 3,
                column: "VideoUrl",
                value: "/assets/images/player/demo_videos.mp4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Episodes",
                keyColumn: "Id",
                keyValue: 1,
                column: "VideoUrl",
                value: "/videos/pilot.mp4");

            migrationBuilder.UpdateData(
                table: "Episodes",
                keyColumn: "Id",
                keyValue: 2,
                column: "VideoUrl",
                value: "/videos/cats-in-the-bag.mp4");

            migrationBuilder.UpdateData(
                table: "Episodes",
                keyColumn: "Id",
                keyValue: 3,
                column: "VideoUrl",
                value: "/videos/chapter-one.mp4");
        }
    }
}
