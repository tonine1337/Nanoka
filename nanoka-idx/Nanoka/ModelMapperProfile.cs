using AutoMapper;
using Nanoka.Models;

namespace Nanoka
{
    public class ModelMapperProfile : Profile
    {
        public ModelMapperProfile()
        {
            // user
            CreateMap<UserBase, User>();
        }
    }
}