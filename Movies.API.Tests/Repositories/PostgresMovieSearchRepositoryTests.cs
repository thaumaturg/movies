using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Movies.API.Data;
using Movies.API.Models.Domain;
using Movies.API.Repositories;

namespace Movies.API.Tests.Repositories;

public class PostgresMovieSearchRepositoryTests : IDisposable
{
    private readonly MoviesDbContext _context;
    private readonly Mock<ILogger<PostgresMovieSearchRepository>> _mockLogger;
    private readonly PostgresMovieSearchRepository _repository;

    public PostgresMovieSearchRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<MoviesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MoviesDbContext(options);
        _mockLogger = new Mock<ILogger<PostgresMovieSearchRepository>>();
        _repository = new PostgresMovieSearchRepository(_context, _mockLogger.Object);
    }

    [Fact]
    public async Task GetLatestSearchesAsync_ReturnsLatestSearches()
    {
        // Arrange
        var searches = new List<MovieSearch>
        {
            new() { Title = "Inception", CreatedAt = DateTime.UtcNow.AddDays(-3) },
            new() { Title = "Interstellar", CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new() { Title = "The Matrix", CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new() { Title = "Blade Runner", CreatedAt = DateTime.UtcNow },
        };

        _context.MovieSearches.AddRange(searches);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetLatestSearchesAsync();

        // Assert
        Assert.Equal(4, result.Count);
        Assert.Equal("Blade Runner", result[0].Title); // Most recent first
        Assert.Equal("The Matrix", result[1].Title);
        Assert.Equal("Interstellar", result[2].Title);
        Assert.Equal("Inception", result[3].Title);
    }

    [Fact]
    public async Task GetLatestSearchesAsync_ReturnsOnlyFive()
    {
        // Arrange
        var searches = new List<MovieSearch>();
        for (int i = 0; i < 10; i++)
        {
            searches.Add(new MovieSearch
            {
                Title = $"Movie {i}",
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }

        _context.MovieSearches.AddRange(searches);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetLatestSearchesAsync();

        // Assert
        Assert.Equal(5, result.Count);
        Assert.Equal("Movie 0", result[0].Title); // Most recent
    }

    [Fact]
    public async Task AddSearchAsync_WithNewTitle_AddsNewSearch()
    {
        // Arrange
        const string title = "Inception";

        // Act
        var result = await _repository.AddSearchAsync(title);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(title, result.Title);

        var searchInDb = await _context.MovieSearches.FirstOrDefaultAsync(s => s.Title == title);
        Assert.NotNull(searchInDb);
        Assert.Equal(title, searchInDb.Title);
    }

    [Fact]
    public async Task AddSearchAsync_WithExistingTitle_UpdatesExistingSearch()
    {
        // Arrange
        const string title = "Inception";
        var existingSearch = new MovieSearch
        {
            Title = title,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _context.MovieSearches.Add(existingSearch);
        await _context.SaveChangesAsync();

        var originalCreatedAt = existingSearch.CreatedAt;

        // Act
        var result = await _repository.AddSearchAsync(title);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(title, result.Title);
        Assert.True(result.CreatedAt > originalCreatedAt);

        var searchCount = await _context.MovieSearches.CountAsync(s => s.Title == title);
        Assert.Equal(1, searchCount); // Should still be only one record
    }

    [Fact]
    public async Task AddSearchAsync_WhenMoreThanFiveSearches_RemovesOldestSearches()
    {
        // Arrange
        var searches = new List<MovieSearch>();
        for (int i = 0; i < 5; i++)
        {
            searches.Add(new MovieSearch
            {
                Title = $"Movie {i}",
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }

        _context.MovieSearches.AddRange(searches);
        await _context.SaveChangesAsync();

        // Act - Add a 6th search
        await _repository.AddSearchAsync("New Movie");

        // Assert
        var totalSearches = await _context.MovieSearches.CountAsync();
        Assert.Equal(5, totalSearches); // Should keep only 5

        var oldestSearch = await _context.MovieSearches
            .FirstOrDefaultAsync(s => s.Title == "Movie 4");
        Assert.Null(oldestSearch); // Oldest should be removed

        var newSearch = await _context.MovieSearches
            .FirstOrDefaultAsync(s => s.Title == "New Movie");
        Assert.NotNull(newSearch); // New search should exist
    }

    [Fact]
    public async Task SearchExistsAsync_WithExistingTitle_ReturnsTrue()
    {
        // Arrange
        const string title = "Inception";
        var search = new MovieSearch { Title = title, CreatedAt = DateTime.UtcNow };

        _context.MovieSearches.Add(search);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchExistsAsync(title);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SearchExistsAsync_WithNonExistingTitle_ReturnsFalse()
    {
        // Act
        var result = await _repository.SearchExistsAsync("Non-existent Movie");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task SearchExistsAsync_IsCaseInsensitive()
    {
        // Arrange
        const string title = "Inception";
        var search = new MovieSearch { Title = title, CreatedAt = DateTime.UtcNow };

        _context.MovieSearches.Add(search);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.SearchExistsAsync("INCEPTION");

        // Assert
        Assert.True(result);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
