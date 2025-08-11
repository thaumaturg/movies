using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Movies.API.Models.Domain;
using Movies.API.Models.DTO;
using Movies.API.Repositories;
using Movies.API.Services;

namespace Movies.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IOmdbService _omdbService;
    private readonly IMovieSearchRepository _searchRepository;
    private readonly ILogger<MoviesController> _logger;
    private readonly IMapper _mapper;

    public MoviesController(
        IOmdbService omdbService,
        IMovieSearchRepository searchRepository,
        ILogger<MoviesController> logger,
        IMapper mapper)
    {
        _omdbService = omdbService;
        _searchRepository = searchRepository;
        _logger = logger;
        _mapper = mapper;
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchMovies([FromQuery] string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return BadRequest("Title parameter is required");
        }

        try
        {
            await _searchRepository.AddSearchAsync(title);

            OmdbSearchResponseDto? searchResult = await _omdbService.SearchMoviesAsync(title);

            if (searchResult == null)
            {
                return StatusCode(500, "Error occurred while searching for movies");
            }

            if (searchResult.Response == "False")
            {
                return NotFound(new { Error = searchResult.Error ?? "No movies found" });
            }

            return Ok(searchResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching for movies with title: {Title}", title);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    [HttpGet("details/{imdbId}")]
    public async Task<IActionResult> GetMovieDetails(string imdbId)
    {
        if (string.IsNullOrWhiteSpace(imdbId))
        {
            return BadRequest("IMDB ID parameter is required");
        }

        try
        {
            OmdbMovieDetailDto? movieDetails = await _omdbService.GetMovieDetailsAsync(imdbId);

            if (movieDetails == null)
            {
                return StatusCode(500, "Error occurred while getting movie details");
            }

            if (movieDetails.Response == "False")
            {
                return NotFound(new { Error = movieDetails.Error ?? "Movie not found" });
            }

            return Ok(movieDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting movie details for IMDB ID: {ImdbId}", imdbId);
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    [HttpGet("search-history")]
    public async Task<IActionResult> GetSearchHistory()
    {
        try
        {
            List<MovieSearch> searches = await _searchRepository.GetLatestSearchesAsync();
            List<MovieSearchDto> searchDtos = _mapper.Map<List<MovieSearchDto>>(searches);

            return Ok(searchDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting search history");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
}
