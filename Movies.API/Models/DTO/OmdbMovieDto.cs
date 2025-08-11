using System.Text.Json.Serialization;

namespace Movies.API.Models.DTO;

public class OmdbMovieDto
{
    [JsonPropertyName("Title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("Year")]
    public string Year { get; set; } = string.Empty;

    [JsonPropertyName("imdbID")]
    public string ImdbId { get; set; } = string.Empty;

    [JsonPropertyName("Type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("Poster")]
    public string Poster { get; set; } = string.Empty;
}
