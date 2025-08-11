namespace Movies.Blazor.Models;

public class MovieDetailDto
{
    public string Title { get; set; } = string.Empty;
    public string Year { get; set; } = string.Empty;
    public string Rated { get; set; } = string.Empty;
    public string Released { get; set; } = string.Empty;
    public string Runtime { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public string Director { get; set; } = string.Empty;
    public string Writer { get; set; } = string.Empty;
    public string Actors { get; set; } = string.Empty;
    public string Plot { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Awards { get; set; } = string.Empty;
    public string Poster { get; set; } = string.Empty;
    public List<RatingDto> Ratings { get; set; } = new();
    public string Metascore { get; set; } = string.Empty;
    public string ImdbRating { get; set; } = string.Empty;
    public string ImdbVotes { get; set; } = string.Empty;
    public string ImdbId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Dvd { get; set; } = string.Empty;
    public string BoxOffice { get; set; } = string.Empty;
    public string Production { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public string? Error { get; set; }
}
