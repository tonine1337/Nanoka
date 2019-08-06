using AutoMapper;
using Nanoka.Core.Models;

namespace Nanoka.Core
{
    public class ModelMapperProfile : Profile
    {
        public ModelMapperProfile()
        {
            // doujinshi
            CreateMap<DoujinshiBase, Doujinshi>();
            CreateMap<DoujinshiVariantBase, DoujinshiVariant>();
            CreateMap<DoujinshiPageBase, DoujinshiPage>();

            // booru
            CreateMap<BooruPostBase, BooruPost>();

            // user
            CreateMap<UserBase, User>();
        }
    }
}
