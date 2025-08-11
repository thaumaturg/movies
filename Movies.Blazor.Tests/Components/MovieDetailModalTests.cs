using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using Movies.Blazor.Components;
using Movies.Blazor.Models;

namespace Movies.Blazor.Tests.Components;

public class MovieDetailModalTests : TestContext
{
    public MovieDetailModalTests()
    {
        var mockJSRuntime = new Mock<IJSRuntime>();
        Services.AddSingleton(mockJSRuntime.Object);
    }

    [Fact]
    public void MovieDetailModal_RendersMovieTitle()
    {
        // Arrange
        var movie = new MovieDetailDto
        {
            Title = "Inception",
            Year = "2010",
            Response = "True"
        };

        // Act
        var component = RenderComponent<MovieDetailModal>(parameters => parameters
            .Add(p => p.Movie, movie));

        // Assert
        Assert.Contains("Inception", component.Markup);
    }

    [Fact]
    public void MovieDetailModal_RendersMovieYear()
    {
        // Arrange
        var movie = new MovieDetailDto
        {
            Title = "Inception",
            Year = "2010",
            Response = "True"
        };

        // Act
        var component = RenderComponent<MovieDetailModal>(parameters => parameters
            .Add(p => p.Movie, movie));

        // Assert
        Assert.Contains("2010", component.Markup);
    }

    [Fact]
    public void MovieDetailModal_RendersPlotWhenAvailable()
    {
        // Arrange
        var movie = new MovieDetailDto
        {
            Title = "Inception",
            Plot = "A thief who steals corporate secrets through the use of dream-sharing technology.",
            Response = "True"
        };

        // Act
        var component = RenderComponent<MovieDetailModal>(parameters => parameters
            .Add(p => p.Movie, movie));

        // Assert
        Assert.Contains("A thief who steals corporate secrets", component.Markup);
    }

    [Fact]
    public void MovieDetailModal_DoesNotRenderPlotWhenNA()
    {
        // Arrange
        var movie = new MovieDetailDto
        {
            Title = "Inception",
            Plot = "N/A",
            Response = "True"
        };

        // Act
        var component = RenderComponent<MovieDetailModal>(parameters => parameters
            .Add(p => p.Movie, movie));

        // Assert
        Assert.DoesNotContain("Plot", component.Markup);
    }

    [Fact]
    public void MovieDetailModal_RendersGenres()
    {
        // Arrange
        var movie = new MovieDetailDto
        {
            Title = "Inception",
            Genre = "Action, Drama, Sci-Fi",
            Response = "True"
        };

        // Act
        var component = RenderComponent<MovieDetailModal>(parameters => parameters
            .Add(p => p.Movie, movie));

        // Assert
        Assert.Contains("Action", component.Markup);
        Assert.Contains("Drama", component.Markup);
        Assert.Contains("Sci-Fi", component.Markup);
    }

    [Fact]
    public void MovieDetailModal_RendersRatings()
    {
        // Arrange
        var movie = new MovieDetailDto
        {
            Title = "Inception",
            Ratings = new List<RatingDto>
            {
                new() { Source = "Internet Movie Database", Value = "8.8/10" },
                new() { Source = "Rotten Tomatoes", Value = "87%" }
            },
            Response = "True"
        };

        // Act
        var component = RenderComponent<MovieDetailModal>(parameters => parameters
            .Add(p => p.Movie, movie));

        // Assert
        Assert.Contains("8.8/10", component.Markup);
        Assert.Contains("Internet Movie Database", component.Markup);
        Assert.Contains("87%", component.Markup);
        Assert.Contains("Rotten Tomatoes", component.Markup);
    }

    [Fact]
    public void MovieDetailModal_RendersImdbRating()
    {
        // Arrange
        var movie = new MovieDetailDto
        {
            Title = "Inception",
            ImdbRating = "8.8",
            ImdbVotes = "2,100,000",
            Response = "True"
        };

        // Act
        var component = RenderComponent<MovieDetailModal>(parameters => parameters
            .Add(p => p.Movie, movie));

        // Assert
        Assert.Contains("8.8", component.Markup);
        Assert.Contains("2,100,000", component.Markup);
        Assert.Contains("IMDB Rating", component.Markup);
    }

    [Fact]
    public void MovieDetailModal_RendersCloseButton()
    {
        // Arrange
        var movie = new MovieDetailDto
        {
            Title = "Inception",
            Response = "True"
        };

        // Act
        var component = RenderComponent<MovieDetailModal>(parameters => parameters
            .Add(p => p.Movie, movie));

        // Assert
        var closeButtons = component.FindAll("button").Where(b => b.TextContent.Contains("Close") || b.TextContent.Contains("×"));
        Assert.True(closeButtons.Any());
    }

    [Fact]
    public void MovieDetailModal_CallsOnCloseWhenCloseButtonClicked()
    {
        // Arrange
        var movie = new MovieDetailDto
        {
            Title = "Inception",
            Response = "True"
        };

        bool onCloseCalled = false;

        // Act
        var component = RenderComponent<MovieDetailModal>(parameters => parameters
            .Add(p => p.Movie, movie)
            .Add(p => p.OnClose, () => onCloseCalled = true));

        var closeButton = component.Find("button:contains('Close')");
        closeButton.Click();

        // Assert
        Assert.True(onCloseCalled);
    }

    [Fact]
    public void MovieDetailModal_DoesNotRenderEmptyOrNAFields()
    {
        // Arrange
        var movie = new MovieDetailDto
        {
            Title = "Inception",
            Director = "N/A",
            Writer = "",
            Actors = "N/A",
            Response = "True"
        };

        // Act
        var component = RenderComponent<MovieDetailModal>(parameters => parameters
            .Add(p => p.Movie, movie));

        // Assert
        Assert.DoesNotContain("Director", component.Markup);
        Assert.DoesNotContain("Writer", component.Markup);
        Assert.DoesNotContain("Cast", component.Markup);
    }

    [Fact]
    public void MovieDetailModal_ShowsNoImageWhenPosterIsNA()
    {
        // Arrange
        var movie = new MovieDetailDto
        {
            Title = "Inception",
            Poster = "N/A",
            Response = "True"
        };

        // Act
        var component = RenderComponent<MovieDetailModal>(parameters => parameters
            .Add(p => p.Movie, movie));

        // Assert
        Assert.Contains("No Image Available", component.Markup);
    }
}
