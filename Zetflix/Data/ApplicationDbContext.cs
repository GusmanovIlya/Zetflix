using Microsoft.EntityFrameworkCore;
using Zetflix.Models;

namespace Zetflix.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Show> Shows => Set<Show>();
    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<Episode> Episodes => Set<Episode>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<MovieGenre> MovieGenres => Set<MovieGenre>();
    public DbSet<ShowGenre> ShowGenres => Set<ShowGenre>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MovieGenre>()
            .HasKey(mg => new { mg.MovieId, mg.GenreId });

        modelBuilder.Entity<ShowGenre>()
            .HasKey(sg => new { sg.ShowId, sg.GenreId });

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin" },
            new Role { Id = 2, Name = "User" }
        );

        modelBuilder.Entity<Subscription>().HasData(
            new Subscription { Id = 1, Name = "Basic", Price = 299, DurationInDays = 30 },
            new Subscription { Id = 2, Name = "Premium", Price = 599, DurationInDays = 30 }
        );

        modelBuilder.Entity<Genre>().HasData(
            new Genre { Id = 1, Name = "Action" },
            new Genre { Id = 2, Name = "Drama" },
            new Genre { Id = 3, Name = "Comedy" }
        );

        modelBuilder.Entity<Movie>().HasData(
            new Movie { Id = 1, Title = "Inception", Description = "A mind-bending sci-fi thriller.", ReleaseYear = 2010, PosterUrl = "/images/movies/inception.jpg", VideoUrl = "/assets/images/video/sample-video.mp4" },
            new Movie { Id = 2, Title = "The Dark Knight", Description = "Batman faces the Joker in Gotham.", ReleaseYear = 2008, PosterUrl = "/images/movies/dark-knight.jpg", VideoUrl = "/assets/images/video/trailer.mp4" },
            new Movie { Id = 3, Title = "Interstellar", Description = "A journey through space and time.", ReleaseYear = 2014, PosterUrl = "/images/movies/interstellar.jpg", VideoUrl = "/assets/images/player/demo_videos.mp4" }
        );

        modelBuilder.Entity<Show>().HasData(
            new Show { Id = 1, Title = "Breaking Bad", Description = "A chemistry teacher turns to crime.", ReleaseYear = 2008, PosterUrl = "/images/shows/breaking-bad.jpg" },
            new Show { Id = 2, Title = "Stranger Things", Description = "Kids uncover supernatural mysteries.", ReleaseYear = 2016, PosterUrl = "/images/shows/stranger-things.jpg" }
        );

        modelBuilder.Entity<Season>().HasData(
            new Season { Id = 1, Number = 1, ShowId = 1 },
            new Season { Id = 2, Number = 1, ShowId = 2 }
        );

        modelBuilder.Entity<Episode>().HasData(
            new Episode { Id = 1, Title = "Pilot", Description = "The story begins.", EpisodeNumber = 1, VideoUrl = "/assets/images/video/sample-video.mp4", SeasonId = 1 },
            new Episode { Id = 2, Title = "Cat's in the Bag...", Description = "Walter deals with consequences.", EpisodeNumber = 2, VideoUrl = "/assets/images/video/trailer.mp4", SeasonId = 1 },
            new Episode { Id = 3, Title = "Chapter One", Description = "A boy disappears in Hawkins.", EpisodeNumber = 1, VideoUrl = "/assets/images/player/demo_videos.mp4", SeasonId = 2 }
        );

        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, UserName = "admin", Email = "admin@zetflix.com", PasswordHash = "admin123", RoleId = 1, SubscriptionId = 2 },
            new User { Id = 2, UserName = "ilya", Email = "ilya@zetflix.com", PasswordHash = "user123", RoleId = 2, SubscriptionId = 1 }
        );

        modelBuilder.Entity<Review>().HasData(
            new Review { Id = 1, Comment = "Amazing movie!", Rating = 5, UserId = 2, MovieId = 1, ShowId = null },
            new Review { Id = 2, Comment = "Very addictive series.", Rating = 5, UserId = 2, MovieId = null, ShowId = 1 }
        );

        modelBuilder.Entity<MovieGenre>().HasData(
            new MovieGenre { MovieId = 1, GenreId = 1 },
            new MovieGenre { MovieId = 1, GenreId = 2 },
            new MovieGenre { MovieId = 2, GenreId = 1 },
            new MovieGenre { MovieId = 3, GenreId = 2 }
        );

        modelBuilder.Entity<ShowGenre>().HasData(
            new ShowGenre { ShowId = 1, GenreId = 2 },
            new ShowGenre { ShowId = 2, GenreId = 1 },
            new ShowGenre { ShowId = 2, GenreId = 3 }
        );
    }
}
