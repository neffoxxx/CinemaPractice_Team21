using AutoMapper;
using AppCore.DTOs;
using Infrastructure.Entities;
namespace AppCore.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Movie, MovieDTO>().ReverseMap();
            CreateMap<Session, SessionDTO>()
                .ForMember(dest => dest.Movies, opt => opt.Ignore())
                .ForMember(dest => dest.Halls, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<Ticket, TicketDTO>()
                .ForMember(dest => dest.MovieTitle, opt => opt.MapFrom(src => src.Session.Movie.Title))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.Username))
                .ForMember(dest => dest.ShowTime, opt => opt.MapFrom(src => src.Session.StartTime))
                .ForMember(dest => dest.HallId, opt => opt.MapFrom(src => src.Session.HallId))
                .ForMember(dest => dest.HallName, opt => opt.MapFrom(src => src.Session.Hall.Name))
                .ReverseMap();
            CreateMap<Hall, HallDTO>().ReverseMap();
        }
    }
}