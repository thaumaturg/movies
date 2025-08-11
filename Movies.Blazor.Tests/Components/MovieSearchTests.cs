using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using Movies.Blazor.Components;
using Movies.Blazor.Models;
using Movies.Blazor.Services;

namespace Movies.Blazor.Tests.Components;

public class MovieSearchTests : TestContext
{
    private readonly Mock<IMovieService> _mockMovieService;
    private readonly Mock<IJSRuntime> _mockJSRuntime;

    public MovieSearchTests()
    {
        _mockMovieService = new Mock<IMovieService>();
        _mockJSRuntime = new Mock<IJSRuntime>();

        Services.AddSingleton(_mockMovieService.Object);
        Services.AddSingleton(_mockJSRuntime.Object);
    }

    [Fact]
    public void MovieSearch_RendersSearchInput()
    {
        // Arrange & Act
        var component = RenderComponent<MovieSearch>();

        // Assert
        var searchInput = component.Find("input[placeholder*='movie title']");
        Assert.NotNull(searchInput);
    }

    [Fact]
    public void MovieSearch_RendersSearchButton()
    {
        // Arrange & Act
        var component = RenderComponent<MovieSearch>();

        // Assert
        var searchButton = component.Find("button[type='submit']");
        Assert.NotNull(searchButton);
        Assert.Contains("Search", searchButton.TextContent);
    }

    [Fact]
    public void MovieSearch_LoadsSearchHistoryOnInitialization()
    {
        // Arrange
        var searchHistory = new List<MovieSearchDto>
        {
            new() { Title = "Inception" },
            new() { Title = "Interstellar" }
        };

        _mockMovieService.Setup(x => x.GetSearchHistoryAsync())
            .ReturnsAsync(searchHistory);

        // Act
        var component = RenderComponent<MovieSearch>();

        // Assert
        _mockMovieService.Verify(x => x.GetSearchHistoryAsync(), Times.Once);
    }

    [Fact]
    public void MovieSearch_DisplaysSearchHistory()
    {
        // Arrange
        var searchHistory = new List<MovieSearchDto>
        {
            new() { Title = "Inception" },
            new() { Title = "Interstellar" }
        };

        _mockMovieService.Setup(x => x.GetSearchHistoryAsync())
            .ReturnsAsync(searchHistory);

        // Act
        var component = RenderComponent<MovieSearch>();

        // Assert
        Assert.Contains("Inception", component.Markup);
        Assert.Contains("Interstellar", component.Markup);
        Assert.Contains("Recent Searches", component.Markup);
    }

    [Fact]
    public void MovieSearch_DisplaysSearchResults()
    {
        // Arrange
        var searchResults = new SearchResponseDto
        {
            Response = "True",
            Search = new List<MovieDto>
            {
                new() { Title = "Inception", Year = "2010", ImdbId = "tt1375666" },
                new() { Title = "Inception: The App", Year = "2018", ImdbId = "tt1375667" }
            }
        };

        _mockMovieService.Setup(x => x.GetSearchHistoryAsync())
            .ReturnsAsync(new List<MovieSearchDto>());
        _mockMovieService.Setup(x => x.SearchMoviesAsync("Inception"))
            .ReturnsAsync(searchResults);

        var component = RenderComponent<MovieSearch>();

        // Act
        var searchInput = component.Find("input");
        searchInput.Input("Inception");

        var searchButton = component.Find("button[type='submit']");
        searchButton.Click();

        // Assert
        Assert.Contains("Inception", component.Markup);
        Assert.Contains("2010", component.Markup);
        Assert.Contains("Search Results (2 movies found)", component.Markup);
    }

    [Fact]
    public void MovieSearch_DisplaysErrorMessage()
    {
        // Arrange
        var errorResponse = new SearchResponseDto
        {
            Response = "False",
            Error = "Movie not found!"
        };

        _mockMovieService.Setup(x => x.GetSearchHistoryAsync())
            .ReturnsAsync(new List<MovieSearchDto>());
        _mockMovieService.Setup(x => x.SearchMoviesAsync("NonExistentMovie"))
            .ReturnsAsync(errorResponse);

        var component = RenderComponent<MovieSearch>();

        // Act
        var searchInput = component.Find("input");
        searchInput.Input("NonExistentMovie");

        var searchButton = component.Find("button[type='submit']");
        searchButton.Click();

        // Assert
        Assert.Contains("Movie not found!", component.Markup);
    }

    [Fact]
    public void MovieSearch_DisplaysLoadingState()
    {
        // Arrange
        var tcs = new TaskCompletionSource<SearchResponseDto?>();

        _mockMovieService.Setup(x => x.GetSearchHistoryAsync())
            .ReturnsAsync(new List<MovieSearchDto>());
        _mockMovieService.Setup(x => x.SearchMoviesAsync("Inception"))
            .Returns(tcs.Task);

        var component = RenderComponent<MovieSearch>();

        // Act
        var searchInput = component.Find("input");
        searchInput.Input("Inception");

        var searchButton = component.Find("button[type='submit']");
        searchButton.Click();

        // Assert
        Assert.Contains("Searching...", component.Markup);
        var disabledButton = component.Find("button[disabled]");
        Assert.NotNull(disabledButton);
    }

    [Fact]
    public void MovieSearch_DoesNotSearchWithEmptyTitle()
    {
        // Arrange
        _mockMovieService.Setup(x => x.GetSearchHistoryAsync())
            .ReturnsAsync(new List<MovieSearchDto>());

        var component = RenderComponent<MovieSearch>();

        // Act
        var searchButton = component.Find("button[type='submit']");
        searchButton.Click();

        // Assert
        _mockMovieService.Verify(x => x.SearchMoviesAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void MovieSearch_CallsMovieDetailsWhenMovieClicked()
    {
        // Arrange
        var movieDetail = new MovieDetailDto
        {
            Response = "True",
            Title = "Inception",
            ImdbId = "tt1375666"
        };

        var searchResults = new SearchResponseDto
        {
            Response = "True",
            Search = new List<MovieDto>
            {
                new() { Title = "Inception", Year = "2010", ImdbId = "tt1375666" }
            }
        };

        _mockMovieService.Setup(x => x.GetSearchHistoryAsync())
            .ReturnsAsync(new List<MovieSearchDto>());
        _mockMovieService.Setup(x => x.SearchMoviesAsync("Inception"))
            .ReturnsAsync(searchResults);
        _mockMovieService.Setup(x => x.GetMovieDetailsAsync("tt1375666"))
            .ReturnsAsync(movieDetail);

        var component = RenderComponent<MovieSearch>();

        // First search for movies
        var searchInput = component.Find("input");
        searchInput.Input("Inception");
        var searchButton = component.Find("button[type='submit']");
        searchButton.Click();

        // Act - Click on a movie
        var movieElement = component.Find("div.bg-gray-50:contains('Inception')");
        movieElement.Click();

        // Assert
        _mockMovieService.Verify(x => x.GetMovieDetailsAsync("tt1375666"), Times.Once);
    }

    [Fact]
    public void MovieSearch_SearchFromHistoryWorksCorrectly()
    {
        // Arrange
        var searchHistory = new List<MovieSearchDto>
        {
            new() { Title = "Inception" }
        };

        var searchResults = new SearchResponseDto
        {
            Response = "True",
            Search = new List<MovieDto>
            {
                new() { Title = "Inception", Year = "2010", ImdbId = "tt1375666" }
            }
        };

        _mockMovieService.Setup(x => x.GetSearchHistoryAsync())
            .ReturnsAsync(searchHistory);
        _mockMovieService.Setup(x => x.SearchMoviesAsync("Inception"))
            .ReturnsAsync(searchResults);

        var component = RenderComponent<MovieSearch>();

        // Act
        var historyButton = component.Find("button:contains('Inception')");
        historyButton.Click();

        // Assert
        _mockMovieService.Verify(x => x.SearchMoviesAsync("Inception"), Times.Once);
    }

    [Fact]
    public void MovieSearch_DisplaysNoResultsMessage()
    {
        // Arrange
        var emptyResults = new SearchResponseDto
        {
            Response = "True",
            Search = new List<MovieDto>()
        };

        _mockMovieService.Setup(x => x.GetSearchHistoryAsync())
            .ReturnsAsync(new List<MovieSearchDto>());
        _mockMovieService.Setup(x => x.SearchMoviesAsync("RandomMovie"))
            .ReturnsAsync(emptyResults);

        var component = RenderComponent<MovieSearch>();

        // Act
        var searchInput = component.Find("input");
        searchInput.Input("RandomMovie");
        var searchButton = component.Find("button[type='submit']");
        searchButton.Click();

        // Assert
        Assert.Contains("No movies found", component.Markup);
    }
}
