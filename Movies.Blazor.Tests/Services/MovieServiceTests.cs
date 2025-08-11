using System.Net;
using System.Text;
using System.Text.Json;
using Moq;
using Moq.Protected;
using Movies.Blazor.Models;
using Movies.Blazor.Services;

namespace Movies.Blazor.Tests.Services;

public class MovieServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly MovieService _service;

    public MovieServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _service = new MovieService(_httpClient);
    }

    [Fact]
    public async Task SearchMoviesAsync_WithValidTitle_ReturnsSearchResponse()
    {
        // Arrange
        const string title = "Inception";
        var expectedResponse = new SearchResponseDto
        {
            Response = "True",
            Search = new List<MovieDto>
            {
                new() { Title = "Inception", Year = "2010", ImdbId = "tt1375666" }
            }
        };

        var json = JsonSerializer.Serialize(expectedResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _service.SearchMoviesAsync(title);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("True", result.Response);
        Assert.Single(result.Search);
        Assert.Equal("Inception", result.Search[0].Title);
    }

    [Fact]
    public async Task SearchMoviesAsync_WhenHttpRequestFails_ReturnsNull()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _service.SearchMoviesAsync("Inception");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetMovieDetailsAsync_WithValidImdbId_ReturnsMovieDetail()
    {
        // Arrange
        const string imdbId = "tt1375666";
        var expectedResponse = new MovieDetailDto
        {
            Response = "True",
            Title = "Inception",
            Year = "2010",
            ImdbId = imdbId,
            Plot = "A thief who steals corporate secrets..."
        };

        var json = JsonSerializer.Serialize(expectedResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _service.GetMovieDetailsAsync(imdbId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("True", result.Response);
        Assert.Equal("Inception", result.Title);
        Assert.Equal(imdbId, result.ImdbId);
    }

    [Fact]
    public async Task GetMovieDetailsAsync_WhenHttpRequestFails_ReturnsNull()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _service.GetMovieDetailsAsync("tt1375666");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetSearchHistoryAsync_WithSuccessfulResponse_ReturnsSearchHistory()
    {
        // Arrange
        var expectedResponse = new List<MovieSearchDto>
        {
            new() { Title = "Inception" },
            new() { Title = "Interstellar" }
        };

        var json = JsonSerializer.Serialize(expectedResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _service.GetSearchHistoryAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("Inception", result[0].Title);
        Assert.Equal("Interstellar", result[1].Title);
    }

    [Fact]
    public async Task GetSearchHistoryAsync_WithUnsuccessfulResponse_ReturnsEmptyList()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        // Act
        var result = await _service.GetSearchHistoryAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetSearchHistoryAsync_WhenHttpRequestFails_ReturnsEmptyList()
    {
        // Arrange
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _service.GetSearchHistoryAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task SearchMoviesAsync_BuildsCorrectUrl()
    {
        // Arrange
        const string title = "Matrix";
        string? capturedUrl = null;

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) =>
            {
                capturedUrl = request.RequestUri?.ToString();
            })
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"Response\":\"True\",\"Search\":[]}", Encoding.UTF8, "application/json")
            });

        // Act
        await _service.SearchMoviesAsync(title);

        // Assert
        Assert.NotNull(capturedUrl);
        Assert.Contains("api/movies/search", capturedUrl);
        Assert.Contains("Matrix", capturedUrl);
    }
}
