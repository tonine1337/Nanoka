using AutoMapper;
using Nanoka.Models;

namespace Nanoka
{
    public class ModelMapperProfile : Profile
    {
        public ModelMapperProfile()
        {
            // book
            CreateMap<BookBase, Book>();
            CreateMap<BookVariantBase, BookVariant>();

            // booru
            CreateMap<BooruPostBase, BooruPost>();

            // user
            CreateMap<UserBase, User>();
        }
    }
}