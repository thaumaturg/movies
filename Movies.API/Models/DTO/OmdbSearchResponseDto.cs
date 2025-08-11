using System.Text.Json.Serialization;

namespace Movies.API.Models.DTO;

public class OmdbSearchResponseDto
{
    [JsonPropertyName("Search")]
    public List<OmdbMovieDto> Search { get; set; } = [];

    [JsonPropertyName("totalResults")]
    public string TotalResults { get; set; } = string.Empty;

    [JsonPropertyName("Response")]
    public string Response { get; set; } = string.Empty;

    [JsonPropertyName("Error")]
    public string? Error { get; set; }
}
