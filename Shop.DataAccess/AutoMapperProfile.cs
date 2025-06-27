using AutoMapper;
using Shop.Core.Models;
using Shop.DataAccess.Entities;

namespace Shop.DataAccess
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UserEntity, User>()
                .ConstructUsing((src, ctx) => User.Create(
                    src.Id,
                    src.UserName,
                    src.PasswordHash,
                    src.Email,
                    src.FirstName,
                    src.LastName,
                    src.PhoneNumber));
        }
    }
} 