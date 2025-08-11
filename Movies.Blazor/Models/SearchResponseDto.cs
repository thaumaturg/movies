namespace Movies.Blazor.Models;

public class SearchResponseDto
{
    public List<MovieDto> Search { get; set; } = new();
    public string TotalResults { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public string? Error { get; set; }
}
