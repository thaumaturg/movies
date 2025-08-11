using Microsoft.EntityFrameworkCore;
using Movies.API.Data;
using Movies.API.Models.Domain;

namespace Movies.API.Repositories;

public class PostgresMovieSearchRepository : IMovieSearchRepository
{
    private readonly MoviesDbContext _context;
    private readonly ILogger<PostgresMovieSearchRepository> _logger;

    public PostgresMovieSearchRepository(MoviesDbContext context, ILogger<PostgresMovieSearchRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<MovieSearch>> GetLatestSearchesAsync()
    {
        try
        {
            return await _context.MovieSearches
                .OrderByDescending(ms => ms.CreatedAt)
                .Take(5)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting latest searches");
            return new List<MovieSearch>();
        }
    }

    public async Task<MovieSearch> AddSearchAsync(string title)
    {
        try
        {
            List<MovieSearch> recentSearches = await GetLatestSearchesAsync();
            MovieSearch? existingSearch = recentSearches.FirstOrDefault(s =>
                s.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

            if (existingSearch != null)
            {
                existingSearch.CreatedAt = DateTime.UtcNow;
                _context.MovieSearches.Update(existingSearch);
                await _context.SaveChangesAsync();
                return existingSearch;
            }

            List<MovieSearch> allSearches = await _context.MovieSearches
                .OrderByDescending(ms => ms.CreatedAt)
                .ToListAsync();

            if (allSearches.Count >= 5)
            {
                var searchesToRemove = allSearches.Skip(4).ToList();
                _context.MovieSearches.RemoveRange(searchesToRemove);
            }

            var newSearch = new MovieSearch
            {
                Title = title,
                CreatedAt = DateTime.UtcNow
            };

            _context.MovieSearches.Add(newSearch);
            await _context.SaveChangesAsync();

            return newSearch;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding search for title: {Title}", title);
            throw;
        }
    }

    public async Task<bool> SearchExistsAsync(string title)
    {
        try
        {
            return await _context.MovieSearches
                .AnyAsync(ms => ms.Title.ToLower() == title.ToLower());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking if search exists for title: {Title}", title);
            return false;
        }
    }
}
