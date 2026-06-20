using AutoMapper;
using ERP_API.Domin.UsersEntity;
using ERPDto.UserDto;
using SherdProject.DTO;
namespace Infrastructure.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Users, UserOut>();
        CreateMap<LoginModel, Users>();
        CreateMap<Users, UserTokenDto>();
        CreateMap<UserTokenDto, UserOut>();
    }
}
