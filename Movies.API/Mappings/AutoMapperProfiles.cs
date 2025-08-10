using AutoMapper;
using Movies.API.Models.Domain;
using Movies.API.Models.Domain.DTO;

namespace Movies.API.Mappings;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<MovieSearch, MovieSearchDto>().ReverseMap();
    }
}
