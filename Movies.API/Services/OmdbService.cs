using System.Text.Json;
using Microsoft.Extensions.Options;
using Movies.API.Models.DTO;

namespace Movies.API.Services;

public class OmdbService : IOmdbService
{
    private readonly HttpClient _httpClient;
    private readonly OmdbConfiguration _configuration;
    private readonly ILogger<OmdbService> _logger;
    private const string BaseUrl = "http://www.omdbapi.com/";

    public OmdbService(HttpClient httpClient, IOptions<OmdbConfiguration> configuration, ILogger<OmdbService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration.Value;
        _logger = logger;
    }

    public async Task<OmdbSearchResponseDto?> SearchMoviesAsync(string title)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_configuration.ApiKey))
            {
                _logger.LogError("OMDB API key is not configured");
                return null;
            }

            string url = $"{BaseUrl}?apikey={_configuration.ApiKey}&s={Uri.EscapeDataString(title)}&page=1&type=movie";

            _logger.LogInformation("Searching for movies with title: {Title}", title);

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string jsonContent = await response.Content.ReadAsStringAsync();
            OmdbSearchResponseDto? searchResponse = JsonSerializer.Deserialize<OmdbSearchResponseDto>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return searchResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching for movies with title: {Title}", title);
            return null;
        }
    }

    public async Task<OmdbMovieDetailDto?> GetMovieDetailsAsync(string imdbId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_configuration.ApiKey))
            {
                _logger.LogError("OMDB API key is not configured");
                return null;
            }

            string url = $"{BaseUrl}?apikey={_configuration.ApiKey}&i={Uri.EscapeDataString(imdbId)}&plot=full";

            _logger.LogInformation("Getting movie details for IMDB ID: {ImdbId}", imdbId);

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string jsonContent = await response.Content.ReadAsStringAsync();
            OmdbMovieDetailDto? movieDetail = JsonSerializer.Deserialize<OmdbMovieDetailDto>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return movieDetail;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting movie details for IMDB ID: {ImdbId}", imdbId);
            return null;
        }
    }
}
