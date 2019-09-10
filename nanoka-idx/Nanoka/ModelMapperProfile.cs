using AutoMapper;
using Nanoka.Models;

namespace Nanoka
{
    public class ModelMapperProfile : Profile
    {
        public ModelMapperProfile()
        {
            CreateMap<BookBase, Book>();
            CreateMap<BookContentBase, BookContent>();
            CreateMap<ImageBase, Image>();
            CreateMap<SongBase, Song>();
            CreateMap<UserBase, User>();
        }
    }
}