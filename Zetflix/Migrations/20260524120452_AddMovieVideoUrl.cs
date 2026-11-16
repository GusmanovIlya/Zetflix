using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Zetflix.Migrations
{
    /// <inheritdoc />
    public partial class AddMovieVideoUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VideoUrl",
                table: "Movies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 1,
                column: "VideoUrl",
                value: "/assets/images/video/sample-video.mp4");

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 2,
                column: "VideoUrl",
                value: "/assets/images/video/trailer.mp4");

            migrationBuilder.UpdateData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 3,
                column: "VideoUrl",
                value: "/assets/images/player/demo_videos.mp4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VideoUrl",
                table: "Movies");
        }
    }
}
