using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Movies.API.Models.DTO;
using Movies.API.Services;

namespace Movies.API.Tests.Services;

public class OmdbServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly Mock<ILogger<OmdbService>> _mockLogger;
    private readonly Mock<IOptions<OmdbConfiguration>> _mockConfiguration;
    private readonly HttpClient _httpClient;
    private readonly OmdbService _service;

    public OmdbServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _mockLogger = new Mock<ILogger<OmdbService>>();
        _mockConfiguration = new Mock<IOptions<OmdbConfiguration>>();

        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);

        var config = new OmdbConfiguration { ApiKey = "test-api-key" };
        _mockConfiguration.Setup(x => x.Value).Returns(config);

        _service = new OmdbService(_httpClient, _mockConfiguration.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task SearchMoviesAsync_WithValidTitle_ReturnsSearchResponse()
    {
        // Arrange
        const string title = "Inception";
        var expectedResponse = new OmdbSearchResponseDto
        {
            Response = "True",
            Search = new List<OmdbMovieDto>
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
    public async Task SearchMoviesAsync_WithNoApiKey_ReturnsNull()
    {
        // Arrange
        var config = new OmdbConfiguration { ApiKey = "" };
        _mockConfiguration.Setup(x => x.Value).Returns(config);
        var service = new OmdbService(_httpClient, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var result = await service.SearchMoviesAsync("Inception");

        // Assert
        Assert.Null(result);
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
        var expectedResponse = new OmdbMovieDetailDto
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
    public async Task GetMovieDetailsAsync_WithNoApiKey_ReturnsNull()
    {
        // Arrange
        var config = new OmdbConfiguration { ApiKey = "" };
        _mockConfiguration.Setup(x => x.Value).Returns(config);
        var service = new OmdbService(_httpClient, _mockConfiguration.Object, _mockLogger.Object);

        // Act
        var result = await service.GetMovieDetailsAsync("tt1375666");

        // Assert
        Assert.Null(result);
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
        Assert.Contains("test-api-key", capturedUrl);
        Assert.Contains("Matrix", capturedUrl);
        Assert.Contains("page=1", capturedUrl);
        Assert.Contains("type=movie", capturedUrl);
    }
}
