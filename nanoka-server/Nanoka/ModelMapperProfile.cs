using AutoMapper;
using Nanoka.Models;

namespace Nanoka
{
    public class ModelMapperProfile : Profile
    {
        public ModelMapperProfile()
        {
            // doujinshi
            CreateMap<DoujinshiBase, Doujinshi>();
            CreateMap<DoujinshiVariantBase, DoujinshiVariant>();

            // booru
            CreateMap<BooruPostBase, BooruPost>();

            // user
            CreateMap<UserBase, User>();
        }
    }
}