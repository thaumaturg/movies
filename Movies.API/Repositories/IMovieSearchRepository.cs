using Movies.API.Models.Domain;

namespace Movies.API.Repositories;

public interface IMovieSearchRepository
{
    Task<List<MovieSearch>> GetLatestSearchesAsync();
    Task<MovieSearch> AddSearchAsync(string title);
    Task<bool> SearchExistsAsync(string title);
}
