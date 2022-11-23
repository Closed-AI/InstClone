using API.Models;
using DAL.Entities;

namespace API.Services
{
    public class LinkGeneratorService
    {
        public Func<PostContent, string?>? LinkContentGenerator;
        public Func<User, string?>? LinkAvatarGenerator;

        public void FixAvatar(User s, UserWithAvatarModel d)
        {
            d.AvatarLink = s.Avatar == null ? null : LinkAvatarGenerator?.Invoke(s);
        }
        public void FixContent(PostContent s, AttachWithLinkModel d)
        {
            d.Link = LinkContentGenerator?.Invoke(s);
        }

    }
}