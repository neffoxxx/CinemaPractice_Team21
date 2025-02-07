using AutoMapper;
using AppCore.DTOs;
using Infrastructure.Entities;
namespace AppCore.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Movie, MovieDTO>()
                .ForMember(dest => dest.SelectedGenreIds, opt => opt.MapFrom(src => 
                    src.MovieGenres.Select(mg => mg.GenreId).ToList()))
                .ForMember(dest => dest.SelectedActorIds, opt => opt.MapFrom(src => 
                    src.MovieActors.Select(ma => ma.ActorId).ToList()));

            CreateMap<MovieDTO, Movie>();

            CreateMap<Session, SessionDTO>()
                .ForMember(dest => dest.MovieTitle, opt => opt.MapFrom(src => 
                    src.Movie != null ? src.Movie.Title : string.Empty))
                .ForMember(dest => dest.HallName, opt => opt.MapFrom(src => 
                    src.Hall != null ? src.Hall.Name : string.Empty));

            CreateMap<SessionDTO, Session>()
                .ForMember(dest => dest.Movie, opt => opt.Ignore())
                .ForMember(dest => dest.Hall, opt => opt.Ignore())
                .ForMember(dest => dest.MovieId, opt => opt.MapFrom(src => src.MovieId))
                .ForMember(dest => dest.HallId, opt => opt.MapFrom(src => src.HallId));

            CreateMap<Ticket, TicketDTO>()
                .ForMember(dest => dest.MovieTitle, opt => opt.MapFrom((src, _, _, context) => 
                    src.Session != null && src.Session.Movie != null ? src.Session.Movie.Title : string.Empty))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom((src, _, _, context) => 
                    src.User != null ? src.User.Username : string.Empty))
                .ForMember(dest => dest.ShowTime, opt => opt.MapFrom((src, _, _, context) => 
                    src.Session != null ? src.Session.StartTime : DateTime.MinValue))
                .ForMember(dest => dest.HallId, opt => opt.MapFrom((src, _, _, context) => 
                    src.Session != null ? src.Session.HallId : 0))
                .ForMember(dest => dest.HallName, opt => opt.MapFrom((src, _, _, context) => 
                    src.Session != null && src.Session.Hall != null ? src.Session.Hall.Name : string.Empty))
                .ReverseMap();
            CreateMap<Hall, HallDTO>().ReverseMap();
            CreateMap<Actor, ActorDTO>().ReverseMap();
            CreateMap<Genre, GenreDTO>().ReverseMap();
        }
    }
}