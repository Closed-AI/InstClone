using API.Models;
using API.Services;
using AutoMapper;
using DAL.Entities;

namespace API.Mapper.MapperActions
{
    public class PostContentMapperAction : IMappingAction<PostContent, AttachWithLinkModel>
    {
        private LinkGeneratorService _links;

        public PostContentMapperAction(LinkGeneratorService linkGeneratorService)
        {
            _links = linkGeneratorService;
        }

        public void Process(PostContent source, AttachWithLinkModel destination, ResolutionContext context)
            => _links.FixContent(source, destination);
    }
}
