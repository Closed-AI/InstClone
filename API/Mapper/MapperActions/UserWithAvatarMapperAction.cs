using API.Models;
using API.Services;
using AutoMapper;
using DAL.Entities;

namespace API.Mapper.MapperActions
{
    public class UserWithAvatarMapperAction : IMappingAction<User, UserWithAvatarModel>
    {
        private LinkGeneratorService _links;

        public UserWithAvatarMapperAction(LinkGeneratorService linkGeneratorService)
        {
            _links = linkGeneratorService;
        }

        public void Process(User source, UserWithAvatarModel destination, ResolutionContext context) =>
            _links.FixAvatar(source, destination);
    }
}