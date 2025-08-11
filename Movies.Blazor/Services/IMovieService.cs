using Movies.Blazor.Models;

namespace Movies.Blazor.Services;

public interface IMovieService
{
    Task<SearchResponseDto?> SearchMoviesAsync(string title);
    Task<MovieDetailDto?> GetMovieDetailsAsync(string imdbId);
    Task<List<MovieSearchDto>?> GetSearchHistoryAsync();
}
