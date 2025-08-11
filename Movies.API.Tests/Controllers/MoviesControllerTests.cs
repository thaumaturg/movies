using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Movies.API.Controllers;
using Movies.API.Models.Domain;
using Movies.API.Models.DTO;
using Movies.API.Repositories;
using Movies.API.Services;

namespace Movies.API.Tests.Controllers;

public class MoviesControllerTests
{
    private readonly Mock<IOmdbService> _mockOmdbService;
    private readonly Mock<IMovieSearchRepository> _mockSearchRepository;
    private readonly Mock<ILogger<MoviesController>> _mockLogger;
    private readonly Mock<IMapper> _mockMapper;
    private readonly MoviesController _controller;

    public MoviesControllerTests()
    {
        _mockOmdbService = new Mock<IOmdbService>();
        _mockSearchRepository = new Mock<IMovieSearchRepository>();
        _mockLogger = new Mock<ILogger<MoviesController>>();
        _mockMapper = new Mock<IMapper>();
        _controller = new MoviesController(
            _mockOmdbService.Object,
            _mockSearchRepository.Object,
            _mockLogger.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task SearchMovies_WithValidTitle_ReturnsOkResult()
    {
        // Arrange
        const string title = "Inception";
        var searchResponse = new OmdbSearchResponseDto
        {
            Response = "True",
            Search = new List<OmdbMovieDto>
            {
                new() { Title = "Inception", Year = "2010", ImdbId = "tt1375666" }
            }
        };

        _mockSearchRepository.Setup(x => x.AddSearchAsync(title))
            .ReturnsAsync(new MovieSearch { Id = 1, Title = title, CreatedAt = DateTime.UtcNow });
        _mockOmdbService.Setup(x => x.SearchMoviesAsync(title))
            .ReturnsAsync(searchResponse);

        // Act
        var result = await _controller.SearchMovies(title);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(searchResponse, okResult.Value);
        _mockSearchRepository.Verify(x => x.AddSearchAsync(title), Times.Once);
        _mockOmdbService.Verify(x => x.SearchMoviesAsync(title), Times.Once);
    }

    [Fact]
    public async Task SearchMovies_WithEmptyTitle_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.SearchMovies("");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Title parameter is required", badRequestResult.Value);
    }

    [Fact]
    public async Task SearchMovies_WithNullTitle_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.SearchMovies(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Title parameter is required", badRequestResult.Value);
    }

    [Fact]
    public async Task SearchMovies_WhenOmdbServiceReturnsNull_ReturnsInternalServerError()
    {
        // Arrange
        const string title = "Inception";
        _mockSearchRepository.Setup(x => x.AddSearchAsync(title))
            .ReturnsAsync(new MovieSearch { Id = 1, Title = title, CreatedAt = DateTime.UtcNow });
        _mockOmdbService.Setup(x => x.SearchMoviesAsync(title))
            .ReturnsAsync((OmdbSearchResponseDto?)null);

        // Act
        var result = await _controller.SearchMovies(title);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
        Assert.Equal("Error occurred while searching for movies", statusResult.Value);
    }

    [Fact]
    public async Task SearchMovies_WhenOmdbServiceReturnsFalse_ReturnsNotFound()
    {
        // Arrange
        const string title = "NonExistentMovie";
        var searchResponse = new OmdbSearchResponseDto
        {
            Response = "False",
            Error = "Movie not found!"
        };

        _mockSearchRepository.Setup(x => x.AddSearchAsync(title))
            .ReturnsAsync(new MovieSearch { Id = 1, Title = title, CreatedAt = DateTime.UtcNow });
        _mockOmdbService.Setup(x => x.SearchMoviesAsync(title))
            .ReturnsAsync(searchResponse);

        // Act
        var result = await _controller.SearchMovies(title);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var errorObject = notFoundResult.Value as dynamic;
        Assert.NotNull(errorObject);
    }

    [Fact]
    public async Task GetMovieDetails_WithValidImdbId_ReturnsOkResult()
    {
        // Arrange
        const string imdbId = "tt1375666";
        var movieDetail = new OmdbMovieDetailDto
        {
            Response = "True",
            Title = "Inception",
            Year = "2010",
            ImdbId = imdbId
        };

        _mockOmdbService.Setup(x => x.GetMovieDetailsAsync(imdbId))
            .ReturnsAsync(movieDetail);

        // Act
        var result = await _controller.GetMovieDetails(imdbId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(movieDetail, okResult.Value);
    }

    [Fact]
    public async Task GetMovieDetails_WithEmptyImdbId_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetMovieDetails("");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("IMDB ID parameter is required", badRequestResult.Value);
    }

    [Fact]
    public async Task GetMovieDetails_WhenServiceReturnsNull_ReturnsInternalServerError()
    {
        // Arrange
        const string imdbId = "tt1375666";
        _mockOmdbService.Setup(x => x.GetMovieDetailsAsync(imdbId))
            .ReturnsAsync((OmdbMovieDetailDto?)null);

        // Act
        var result = await _controller.GetMovieDetails(imdbId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetSearchHistory_ReturnsOkResult()
    {
        // Arrange
        var searches = new List<MovieSearch>
        {
            new() { Id = 1, Title = "Inception", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Title = "Interstellar", CreatedAt = DateTime.UtcNow }
        };
        var searchDtos = new List<MovieSearchDto>
        {
            new() { Title = "Inception" },
            new() { Title = "Interstellar" }
        };

        _mockSearchRepository.Setup(x => x.GetLatestSearchesAsync())
            .ReturnsAsync(searches);
        _mockMapper.Setup(x => x.Map<List<MovieSearchDto>>(searches))
            .Returns(searchDtos);

        // Act
        var result = await _controller.GetSearchHistory();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(searchDtos, okResult.Value);
    }

    [Fact]
    public async Task SearchMovies_WhenExceptionOccurs_ReturnsInternalServerError()
    {
        // Arrange
        const string title = "Inception";
        _mockSearchRepository.Setup(x => x.AddSearchAsync(title))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.SearchMovies(title);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
        Assert.Equal("An error occurred while processing your request", statusResult.Value);
    }
}
