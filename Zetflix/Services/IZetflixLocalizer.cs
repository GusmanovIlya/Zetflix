namespace Zetflix.Services;

public interface IZetflixLocalizer
{
    string this[string key] { get; }
}
