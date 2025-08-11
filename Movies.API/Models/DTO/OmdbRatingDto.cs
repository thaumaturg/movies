using System.Text.Json.Serialization;

namespace Movies.API.Models.DTO;

public class OmdbRatingDto
{
    [JsonPropertyName("Source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("Value")]
    public string Value { get; set; } = string.Empty;
}
