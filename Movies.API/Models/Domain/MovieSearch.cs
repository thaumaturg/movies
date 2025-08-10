namespace Movies.API.Models.Domain;

public class MovieSearch
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public DateTime CreatedAt { get; set; }
}
