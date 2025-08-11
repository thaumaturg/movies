using System.Text.Json;
using Movies.Blazor.Models;

namespace Movies.Blazor.Services;

public class MovieService : IMovieService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public MovieService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<SearchResponseDto?> SearchMoviesAsync(string title)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:7067/api/movies/search?title={Uri.EscapeDataString(title)}");

            string jsonContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<SearchResponseDto>(jsonContent, _jsonOptions);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<MovieDetailDto?> GetMovieDetailsAsync(string imdbId)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:7067/api/movies/details/{Uri.EscapeDataString(imdbId)}");

            string jsonContent = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<MovieDetailDto>(jsonContent, _jsonOptions);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<List<MovieSearchDto>?> GetSearchHistoryAsync()
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync("https://localhost:7067/api/movies/search-history");

            if (response.IsSuccessStatusCode)
            {
                string jsonContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<MovieSearchDto>>(jsonContent, _jsonOptions);
            }

            return new List<MovieSearchDto>();
        }
        catch (Exception)
        {
            return new List<MovieSearchDto>();
        }
    }
}
