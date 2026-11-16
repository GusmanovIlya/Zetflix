using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Zetflix.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Genres",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Action" },
                    { 2, "Drama" },
                    { 3, "Comedy" }
                });

            migrationBuilder.InsertData(
                table: "Movies",
                columns: new[] { "Id", "Description", "PosterUrl", "ReleaseYear", "Title" },
                values: new object[,]
                {
                    { 1, "A mind-bending sci-fi thriller.", "/images/movies/inception.jpg", 2010, "Inception" },
                    { 2, "Batman faces the Joker in Gotham.", "/images/movies/dark-knight.jpg", 2008, "The Dark Knight" },
                    { 3, "A journey through space and time.", "/images/movies/interstellar.jpg", 2014, "Interstellar" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Admin" },
                    { 2, "User" }
                });

            migrationBuilder.InsertData(
                table: "Shows",
                columns: new[] { "Id", "Description", "PosterUrl", "ReleaseYear", "Title" },
                values: new object[,]
                {
                    { 1, "A chemistry teacher turns to crime.", "/images/shows/breaking-bad.jpg", 2008, "Breaking Bad" },
                    { 2, "Kids uncover supernatural mysteries.", "/images/shows/stranger-things.jpg", 2016, "Stranger Things" }
                });

            migrationBuilder.InsertData(
                table: "Subscriptions",
                columns: new[] { "Id", "DurationInDays", "Name", "Price" },
                values: new object[,]
                {
                    { 1, 30, "Basic", 299m },
                    { 2, 30, "Premium", 599m }
                });

            migrationBuilder.InsertData(
                table: "MovieGenres",
                columns: new[] { "GenreId", "MovieId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 1 },
                    { 1, 2 },
                    { 2, 3 }
                });

            migrationBuilder.InsertData(
                table: "Seasons",
                columns: new[] { "Id", "Number", "ShowId" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 1, 2 }
                });

            migrationBuilder.InsertData(
                table: "ShowGenres",
                columns: new[] { "GenreId", "ShowId" },
                values: new object[,]
                {
                    { 2, 1 },
                    { 1, 2 },
                    { 3, 2 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "PasswordHash", "RoleId", "SubscriptionId", "UserName" },
                values: new object[,]
                {
                    { 1, "admin@zetflix.com", "admin123", 1, 2, "admin" },
                    { 2, "ilya@zetflix.com", "user123", 2, 1, "ilya" }
                });

            migrationBuilder.InsertData(
                table: "Episodes",
                columns: new[] { "Id", "Description", "EpisodeNumber", "SeasonId", "Title", "VideoUrl" },
                values: new object[,]
                {
                    { 1, "The story begins.", 1, 1, "Pilot", "/videos/pilot.mp4" },
                    { 2, "Walter deals with consequences.", 2, 1, "Cat's in the Bag...", "/videos/cats-in-the-bag.mp4" },
                    { 3, "A boy disappears in Hawkins.", 1, 2, "Chapter One", "/videos/chapter-one.mp4" }
                });

            migrationBuilder.InsertData(
                table: "Reviews",
                columns: new[] { "Id", "Comment", "MovieId", "Rating", "ShowId", "UserId" },
                values: new object[,]
                {
                    { 1, "Amazing movie!", 1, 5, null, 2 },
                    { 2, "Very addictive series.", null, 5, 1, 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Episodes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Episodes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Episodes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MovieGenres",
                keyColumns: new[] { "GenreId", "MovieId" },
                keyValues: new object[] { 1, 1 });

            migrationBuilder.DeleteData(
                table: "MovieGenres",
                keyColumns: new[] { "GenreId", "MovieId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                table: "MovieGenres",
                keyColumns: new[] { "GenreId", "MovieId" },
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                table: "MovieGenres",
                keyColumns: new[] { "GenreId", "MovieId" },
                keyValues: new object[] { 2, 3 });

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ShowGenres",
                keyColumns: new[] { "GenreId", "ShowId" },
                keyValues: new object[] { 2, 1 });

            migrationBuilder.DeleteData(
                table: "ShowGenres",
                keyColumns: new[] { "GenreId", "ShowId" },
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                table: "ShowGenres",
                keyColumns: new[] { "GenreId", "ShowId" },
                keyValues: new object[] { 3, 2 });

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Genres",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Movies",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Seasons",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Seasons",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Subscriptions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Shows",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Shows",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Subscriptions",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
