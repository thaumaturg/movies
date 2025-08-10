namespace Movies.API.Models.Domain;

public class MovieSearch
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public required DateTime CreatedAt { get; set; }
}
