using API.Mapper.MapperActions;
using API.Models;
using AutoMapper;
using Common;
using DAL.Entities;

namespace API.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<CreateUserModel, User>()
                .ForMember(d => d.Id, m => m.MapFrom(s => Guid.NewGuid()))
                .ForMember(d => d.PasswordHash, m => m.MapFrom(s => HashHelper.GetHash(s.Password)))
                .ForMember(d => d.BirthDate, m => m.MapFrom(s => s.BirthDate.UtcDateTime))
                ;
            CreateMap<User, UserModel>();
            CreateMap<User, UserWithAvatarModel>()
                .AfterMap<UserWithAvatarMapperAction>()
                ;

            CreateMap<Post, PostModel>()
                .ForMember(d => d.Contents, m => m.MapFrom(s => s.PostContent))
                .ForMember(d => d.LikeCount, m => m.MapFrom(s => s.Likes != null ? s.Likes.Count : 0));
                ;
            CreateMap<CreatePostModel, Post>()
                .ForMember(d => d.PostContent, m => m.MapFrom(s => s.Contents))
                .ForMember(d => d.CreatingDate, m => m.MapFrom(s => DateTime.UtcNow))
                ;
            CreateMap<CreatePostRequest, CreatePostModel>();

            CreateMap<Comment, CommentModel>()
                .ForMember(d => d.LikeCount, m => m.MapFrom(s => s.Likes != null ? s.Likes.Count : 0))
                ;
            CreateMap<CreateCommentRequest, CreateCommentModel>();
            CreateMap<CreateCommentModel, Comment>()
                .ForMember(d => d.CreatingDate, m => m.MapFrom(s => DateTime.UtcNow))
                ;

            CreateMap<Avatar, AttachModel>();
            CreateMap<PostContent, AttachModel>();
            CreateMap<PostContent, AttachWithLinkModel>().AfterMap<PostContentMapperAction>();

            CreateMap<MetadataModel, MetadataLinkModel>();
            CreateMap<MetadataLinkModel, PostContent>();
        }
    }
}
