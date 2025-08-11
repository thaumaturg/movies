using Movies.API.Models.DTO;

namespace Movies.API.Services;

public interface IOmdbService
{
    Task<OmdbSearchResponseDto?> SearchMoviesAsync(string title);
    Task<OmdbMovieDetailDto?> GetMovieDetailsAsync(string imdbId);
}
