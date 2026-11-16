using Microsoft.EntityFrameworkCore;
using Zetflix.Models;
using Zetflix.Services;

namespace Zetflix.Data;

public static class CatalogSeeder
{
    private sealed record MovieSeed(string Title, string Description, int ReleaseYear, string PosterUrl, string VideoUrl, string[] Genres);

    private sealed record ShowSeed(string Title, string Description, int ReleaseYear, string PosterUrl, string[] Genres, SeasonSeed[] Seasons);

    private sealed record SeasonSeed(int Number, EpisodeSeed[] Episodes);

    private sealed record EpisodeSeed(int Number, string Title, string Description);

    public static async Task SeedAsync(IServiceProvider services)
    {
        try
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            if (!await context.Database.CanConnectAsync(timeout.Token))
            {
                logger.LogWarning("Catalog seed skipped because the database is not reachable.");
                return;
            }

            await SeedRolesAndUsersAsync(context);
            await SeedGenresAsync(context);
            await SeedMoviesAsync(context);
            await SeedShowsAsync(context);
            await SeedReviewsAsync(context);
        }
        catch (Exception exception)
        {
            using var scope = services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
            logger.LogWarning(exception, "Catalog seed was skipped.");
        }
    }

    private static async Task SeedRolesAndUsersAsync(ApplicationDbContext context)
    {
        var moderatorRole = await context.Roles.FirstOrDefaultAsync(role => role.Name == "Moderator");
        if (moderatorRole == null)
        {
            moderatorRole = new Role { Name = "Moderator" };
            context.Roles.Add(moderatorRole);
            await context.SaveChangesAsync();
        }

        var moderatorExists = await context.Users.AnyAsync(user => user.Email == "moderator@zetflix.com");
        if (!moderatorExists)
        {
            context.Users.Add(new User
            {
                UserName = "moderator",
                Email = "moderator@zetflix.com",
                PasswordHash = "moderator123",
                RoleId = moderatorRole.Id,
                SubscriptionId = null
            });

            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedGenresAsync(ApplicationDbContext context)
    {
        var genreNames = new[]
        {
            "Action",
            "Drama",
            "Comedy",
            "Sci-Fi",
            "Thriller",
            "Adventure",
            "Animation",
            "Crime",
            "Fantasy",
            "Horror",
            "Mystery",
            "Family"
        };

        var existing = await context.Genres.Select(genre => genre.Name).ToListAsync();
        foreach (var genreName in genreNames)
        {
            if (!existing.Any(name => string.Equals(name, genreName, StringComparison.OrdinalIgnoreCase)))
            {
                context.Genres.Add(new Genre { Name = genreName });
            }
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedMoviesAsync(ApplicationDbContext context)
    {
        var posterFixes = new Dictionary<string, string>
        {
            ["Inception"] = "/assets/images/generated-posters/sci-fi-arrival.jpg",
            ["The Dark Knight"] = "/assets/images/generated-posters/urban-action.jpg",
            ["Interstellar"] = "/assets/images/generated-posters/space-drama.jpg"
        };

        var existingMovies = await context.Movies
            .Include(movie => movie.MovieGenres)
            .ToListAsync();

        foreach (var movie in existingMovies)
        {
            if (posterFixes.TryGetValue(movie.Title, out var posterUrl))
            {
                movie.PosterUrl = posterUrl;
            }

            if (PlayableVideoUrl.NormalizeForStorage(movie.VideoUrl) != movie.VideoUrl)
            {
                movie.VideoUrl = GetDefaultMovieVideoUrl(movie.Id);
            }
        }

        var movies = new[]
        {
            new MovieSeed("John Wick", "Бывший наемник возвращается в игру после удара по самому личному.", 2014, "/assets/images/generated-posters/urban-action.jpg", "/assets/images/video/trailer.mp4", new[] { "Action", "Thriller", "Crime" }),
            new MovieSeed("Spider-Man: New Swing", "Юный герой учится совмещать обычную жизнь и ответственность большого города.", 2021, "/assets/images/generated-posters/animated-comedy.jpg", "/assets/images/video/sample-video.mp4", new[] { "Action", "Adventure", "Comedy" }),
            new MovieSeed("Arrival", "Лингвист пытается понять язык гостей из космоса, пока мир стоит на грани паники.", 2016, "/assets/images/generated-posters/sci-fi-arrival.jpg", "/assets/images/player/demo_videos.mp4", new[] { "Sci-Fi", "Drama", "Mystery" }),
            new MovieSeed("Bumblebee", "История дружбы, скорости и большого робота, который ищет свое место на Земле.", 2018, "/assets/images/generated-posters/space-drama.jpg", "/assets/images/video/sample-video.mp4", new[] { "Action", "Adventure", "Family" }),
            new MovieSeed("Kung Fu Panda", "Неуклюжий мечтатель получает шанс стать воином, хотя никто в него не верит.", 2008, "/assets/images/generated-posters/animated-comedy.jpg", "/assets/images/video/trailer.mp4", new[] { "Animation", "Comedy", "Family" }),
            new MovieSeed("Deadpool", "Супергеройский хаос с острым языком, быстрым темпом и большим количеством проблем.", 2016, "/assets/images/generated-posters/urban-action.jpg", "/assets/images/video/trailer.mp4", new[] { "Action", "Comedy" }),
            new MovieSeed("Joker", "Мрачная история человека, который постепенно превращает боль в опасный образ.", 2019, "/assets/images/generated-posters/horror-house.jpg", "/assets/images/player/demo_videos.mp4", new[] { "Drama", "Crime", "Thriller" }),
            new MovieSeed("Venom", "Журналист и инопланетный симбиот пытаются договориться внутри одного тела.", 2018, "/assets/images/generated-posters/space-drama.jpg", "/assets/images/video/sample-video.mp4", new[] { "Action", "Sci-Fi", "Thriller" }),
            new MovieSeed("The Conjuring", "Семья сталкивается с пугающей силой в доме, где прошлое не отпускает.", 2013, "/assets/images/generated-posters/horror-house.jpg", "/assets/images/player/demo_videos.mp4", new[] { "Horror", "Mystery", "Thriller" }),
            new MovieSeed("Titanic", "Большая история любви разворачивается на фоне одного из самых известных путешествий.", 1997, "/assets/images/generated-posters/sci-fi-arrival.jpg", "/assets/images/video/sample-video.mp4", new[] { "Drama", "Adventure" }),
            new MovieSeed("War for the Planet", "Последняя битва за будущее начинается там, где заканчивается надежда.", 2017, "/assets/images/generated-posters/dragon-realm.jpg", "/assets/images/video/trailer.mp4", new[] { "Action", "Drama", "Sci-Fi" }),
            new MovieSeed("Pirates Day One", "Морское приключение с тайнами, предательствами и охотой за легендарной картой.", 2020, "/assets/images/generated-posters/dragon-realm.jpg", "/assets/images/player/demo_videos.mp4", new[] { "Adventure", "Fantasy", "Comedy" }),
            new MovieSeed("Frozen Lights", "Сказочная история о семье, магии и силе, которая возвращает тепло в королевство.", 2019, "/assets/images/generated-posters/frozen-lights.jpg", "/assets/images/video/sample-video.mp4", new[] { "Animation", "Family", "Adventure" }),
            new MovieSeed("Minions: Yellow Mission", "Маленькие герои устраивают большое приключение, где план снова идет не по плану.", 2022, "/assets/images/generated-posters/animated-comedy.jpg", "/assets/images/video/sample-video.mp4", new[] { "Animation", "Comedy", "Family" }),
            new MovieSeed("Assassin's Creed", "Воин из прошлого открывает тайну ордена и собственную связь с древним конфликтом.", 2016, "/assets/images/generated-posters/urban-action.jpg", "/assets/images/video/trailer.mp4", new[] { "Action", "Adventure", "Sci-Fi" }),
            new MovieSeed("Fast Furious", "Команда возвращается на трассу, где скорость решает больше, чем слова.", 2021, "/assets/images/generated-posters/fast-action.jpg", "/assets/images/video/trailer.mp4", new[] { "Action", "Crime", "Thriller" }),
            new MovieSeed("Dinosaur Island", "Экспедиция попадает на остров, где древняя жизнь оказалась слишком настоящей.", 2024, "/assets/images/generated-posters/dragon-realm.jpg", "/assets/images/player/demo_videos.mp4", new[] { "Adventure", "Sci-Fi", "Thriller" }),
            new MovieSeed("Migration", "Семейное путешествие превращается в яркое приключение через города и океаны.", 2023, "/assets/images/generated-posters/animated-comedy.jpg", "/assets/images/video/sample-video.mp4", new[] { "Animation", "Comedy", "Family" }),
            new MovieSeed("The Hunter", "Охотник идет по следу дела, которое связывает город, лес и давнюю пропажу.", 2020, "/assets/images/generated-posters/horror-house.jpg", "/assets/images/player/demo_videos.mp4", new[] { "Thriller", "Mystery", "Drama" }),
            new MovieSeed("Red", "Бывшие агенты снова собираются вместе, когда спокойная жизнь резко заканчивается.", 2010, "/assets/images/generated-posters/fast-action.jpg", "/assets/images/video/trailer.mp4", new[] { "Action", "Comedy", "Crime" })
        };

        await UpsertMoviesAsync(context, movies);
    }

    private static async Task UpsertMoviesAsync(ApplicationDbContext context, MovieSeed[] movies)
    {
        var genres = await context.Genres.ToDictionaryAsync(genre => genre.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var movieSeed in movies)
        {
            var movie = await context.Movies
                .Include(currentMovie => currentMovie.MovieGenres)
                .FirstOrDefaultAsync(currentMovie => currentMovie.Title == movieSeed.Title);

            if (movie == null)
            {
                movie = new Movie
                {
                    Title = movieSeed.Title,
                    Description = movieSeed.Description,
                    ReleaseYear = movieSeed.ReleaseYear,
                    PosterUrl = movieSeed.PosterUrl,
                    VideoUrl = movieSeed.VideoUrl
                };
                context.Movies.Add(movie);
                await context.SaveChangesAsync();
            }
            else
            {
                movie.Description = movieSeed.Description;
                movie.ReleaseYear = movieSeed.ReleaseYear;
                movie.PosterUrl = movieSeed.PosterUrl;
                movie.VideoUrl = movieSeed.VideoUrl;
            }

            foreach (var genreName in movieSeed.Genres)
            {
                if (genres.TryGetValue(genreName, out var genre) && movie.MovieGenres.All(movieGenre => movieGenre.GenreId != genre.Id))
                {
                    movie.MovieGenres.Add(new MovieGenre { MovieId = movie.Id, GenreId = genre.Id });
                }
            }
        }

        await context.SaveChangesAsync();
    }

    private static string GetDefaultMovieVideoUrl(int movieId)
    {
        return movieId % 3 == 0
            ? "/assets/images/player/demo_videos.mp4"
            : movieId % 2 == 0
                ? "/assets/images/video/trailer.mp4"
                : "/assets/images/video/sample-video.mp4";
    }

    private static async Task SeedShowsAsync(ApplicationDbContext context)
    {
        var posterFixes = new Dictionary<string, string>
        {
            ["Breaking Bad"] = "/assets/images/generated-posters/urban-action.jpg",
            ["Stranger Things"] = "/assets/images/generated-posters/horror-house.jpg",
            ["Castle Rock"] = "/assets/images/generated-posters/horror-house.jpg",
            ["The First of Us"] = "/assets/images/generated-posters/space-drama.jpg",
            ["Dragon Realm"] = "/assets/images/generated-posters/dragon-realm.jpg",
            ["One Peoc"] = "/assets/images/generated-posters/animated-comedy.jpg",
            ["Synchronic"] = "/assets/images/generated-posters/sci-fi-arrival.jpg"
        };

        var existingShows = await context.Shows.ToListAsync();
        foreach (var show in existingShows)
        {
            if (posterFixes.TryGetValue(show.Title, out var posterUrl))
            {
                show.PosterUrl = posterUrl;
            }
        }

        var existingEpisodes = await context.Episodes.ToListAsync();
        foreach (var episode in existingEpisodes)
        {
            if (PlayableVideoUrl.NormalizeForStorage(episode.VideoUrl) != episode.VideoUrl)
            {
                episode.VideoUrl = GetDefaultEpisodeVideoUrl(episode.Id);
            }
        }

        var shows = new[]
        {
            new ShowSeed("Castle Rock", "Маленький город собирает большие тайны, где каждая улица хранит чужие страхи.", 2018, "/assets/images/generated-posters/horror-house.jpg", new[] { "Drama", "Horror", "Mystery" }, new[]
            {
                new SeasonSeed(1, new[]
                {
                    new EpisodeSeed(1, "Severance", "Новый заключенный меняет баланс сил в городе."),
                    new EpisodeSeed(2, "Habeas Corpus", "Расследование становится слишком личным."),
                    new EpisodeSeed(3, "Local Color", "Старые воспоминания получают новые доказательства.")
                }),
                new SeasonSeed(2, new[]
                {
                    new EpisodeSeed(1, "Let the River Run", "Город снова встречает незнакомку с прошлым."),
                    new EpisodeSeed(2, "New Jerusalem", "Секреты семей выходят на поверхность.")
                })
            }),
            new ShowSeed("The First of Us", "После катастрофы двое идут через опасную страну и учатся доверять друг другу.", 2023, "/assets/images/generated-posters/space-drama.jpg", new[] { "Drama", "Adventure", "Thriller" }, new[]
            {
                new SeasonSeed(1, new[]
                {
                    new EpisodeSeed(1, "When You're Lost", "Путь начинается с тяжелого выбора."),
                    new EpisodeSeed(2, "Infected", "Город показывает, насколько изменилась жизнь."),
                    new EpisodeSeed(3, "Long Road", "Новая встреча дает надежду и боль.")
                })
            }),
            new ShowSeed("Dragon Realm", "Королевства, магия и древние драконы сталкиваются в борьбе за трон.", 2022, "/assets/images/generated-posters/dragon-realm.jpg", new[] { "Fantasy", "Adventure", "Drama" }, new[]
            {
                new SeasonSeed(1, new[]
                {
                    new EpisodeSeed(1, "Awakening of the Drakes", "Древний огонь возвращается в мир."),
                    new EpisodeSeed(2, "Kings and Conspiracies", "Союзы меняются быстрее, чем клятвы."),
                    new EpisodeSeed(3, "Fire and Bloodlines", "Наследники узнают цену власти.")
                }),
                new SeasonSeed(2, new[]
                {
                    new EpisodeSeed(1, "Dragon's Reckoning", "Королевство готовится к новой войне."),
                    new EpisodeSeed(2, "Forged Alliances", "Враги садятся за один стол.")
                })
            }),
            new ShowSeed("One Peoc", "Команда искателей приключений отправляется за сокровищем, которое меняет судьбы.", 2021, "/assets/images/generated-posters/animated-comedy.jpg", new[] { "Adventure", "Comedy", "Fantasy" }, new[]
            {
                new SeasonSeed(1, new[]
                {
                    new EpisodeSeed(1, "Setting Sail", "Команда впервые выходит в море."),
                    new EpisodeSeed(2, "Hidden Allies", "Неожиданный союз спасает экспедицию."),
                    new EpisodeSeed(3, "Shipwrecked", "План рушится после шторма.")
                })
            }),
            new ShowSeed("Synchronic", "Детективы расследуют дела, где время ведет себя слишком странно.", 2020, "/assets/images/generated-posters/sci-fi-arrival.jpg", new[] { "Sci-Fi", "Mystery", "Thriller" }, new[]
            {
                new SeasonSeed(1, new[]
                {
                    new EpisodeSeed(1, "Contact", "Первое дело выводит команду за границы обычной логики."),
                    new EpisodeSeed(2, "Final Transmission", "Сигнал из прошлого раскрывает угрозу."),
                    new EpisodeSeed(3, "Contingencies", "Каждое решение запускает новую версию событий.")
                })
            })
        };

        await UpsertShowsAsync(context, shows);
    }

    private static async Task UpsertShowsAsync(ApplicationDbContext context, ShowSeed[] shows)
    {
        var genres = await context.Genres.ToDictionaryAsync(genre => genre.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var showSeed in shows)
        {
            var show = await context.Shows
                .Include(currentShow => currentShow.ShowGenres)
                .Include(currentShow => currentShow.Seasons)
                    .ThenInclude(season => season.Episodes)
                .FirstOrDefaultAsync(currentShow => currentShow.Title == showSeed.Title);

            if (show == null)
            {
                show = new Show
                {
                    Title = showSeed.Title,
                    Description = showSeed.Description,
                    ReleaseYear = showSeed.ReleaseYear,
                    PosterUrl = showSeed.PosterUrl
                };
                context.Shows.Add(show);
                await context.SaveChangesAsync();
            }
            else
            {
                show.Description = showSeed.Description;
                show.ReleaseYear = showSeed.ReleaseYear;
                show.PosterUrl = showSeed.PosterUrl;
            }

            foreach (var genreName in showSeed.Genres)
            {
                if (genres.TryGetValue(genreName, out var genre) && show.ShowGenres.All(showGenre => showGenre.GenreId != genre.Id))
                {
                    show.ShowGenres.Add(new ShowGenre { ShowId = show.Id, GenreId = genre.Id });
                }
            }

            foreach (var seasonSeed in showSeed.Seasons)
            {
                var season = show.Seasons.FirstOrDefault(currentSeason => currentSeason.Number == seasonSeed.Number);
                if (season == null)
                {
                    season = new Season { Number = seasonSeed.Number, ShowId = show.Id };
                    show.Seasons.Add(season);
                    await context.SaveChangesAsync();
                }

                foreach (var episodeSeed in seasonSeed.Episodes)
                {
                    var episode = season.Episodes.FirstOrDefault(currentEpisode => currentEpisode.EpisodeNumber == episodeSeed.Number);
                    if (episode == null)
                    {
                        season.Episodes.Add(new Episode
                        {
                            Title = episodeSeed.Title,
                            Description = episodeSeed.Description,
                            EpisodeNumber = episodeSeed.Number,
                            VideoUrl = GetDefaultEpisodeVideoUrl(episodeSeed.Number),
                            SeasonId = season.Id
                        });
                    }
                    else
                    {
                        episode.Title = episodeSeed.Title;
                        episode.Description = episodeSeed.Description;
                        episode.VideoUrl = GetDefaultEpisodeVideoUrl(episode.Id);
                    }
                }
            }
        }

        await context.SaveChangesAsync();
    }

    private static string GetDefaultEpisodeVideoUrl(int episodeId)
    {
        return episodeId % 3 == 0
            ? "/assets/images/player/demo_videos.mp4"
            : episodeId % 2 == 0
                ? "/assets/images/video/trailer.mp4"
                : "/assets/images/video/sample-video.mp4";
    }

    private static async Task SeedReviewsAsync(ApplicationDbContext context)
    {
        var user = await context.Users.FirstOrDefaultAsync(currentUser => currentUser.Email == "ilya@zetflix.com");
        if (user == null)
        {
            return;
        }

        var movieTitles = new[] { "John Wick", "Arrival", "Kung Fu Panda", "Joker" };
        foreach (var title in movieTitles)
        {
            var movie = await context.Movies.FirstOrDefaultAsync(currentMovie => currentMovie.Title == title);
            if (movie != null && !await context.Reviews.AnyAsync(review => review.UserId == user.Id && review.MovieId == movie.Id))
            {
                context.Reviews.Add(new Review
                {
                    UserId = user.Id,
                    MovieId = movie.Id,
                    Rating = title == "Joker" ? 4 : 5,
                    Comment = title == "Kung Fu Panda"
                        ? "Отличный фильм для легкого вечера."
                        : "Смотрится уверенно и хорошо оживляет каталог."
                });
            }
        }

        var showTitles = new[] { "Castle Rock", "Dragon Realm", "The First of Us" };
        foreach (var title in showTitles)
        {
            var show = await context.Shows.FirstOrDefaultAsync(currentShow => currentShow.Title == title);
            if (show != null && !await context.Reviews.AnyAsync(review => review.UserId == user.Id && review.ShowId == show.Id))
            {
                context.Reviews.Add(new Review
                {
                    UserId = user.Id,
                    ShowId = show.Id,
                    Rating = 5,
                    Comment = "Есть что открыть, сезоны и эпизоды выглядят наполненно."
                });
            }
        }

        await context.SaveChangesAsync();
    }
}
