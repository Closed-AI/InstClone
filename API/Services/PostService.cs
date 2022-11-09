using API.Models;
using DAL.Entities;
using DAL;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class PostService
    {
        private readonly DAL.DataContext _dataContext;
        private readonly UserService _userService;

        public PostService(DataContext dataContext, UserService userService)
        {
            _dataContext = dataContext;
            _userService = userService;
        }

        public async Task CreatePost(Guid creatorId,CreatePostModel model)
        {
            var post = new Post
            {
                Id = Guid.NewGuid(),
                Description = model.Description,
                CreatingDate = DateTime.UtcNow,
                CreatorId = creatorId
            };
            await _dataContext.Posts.AddAsync(post);
            await _dataContext.SaveChangesAsync();

            if (model.Contents != null)
                foreach (var el in model.Contents)
                {
                    await AddContentToPost(post, el);
                }
        }

        private async Task AddContentToPost(Post post, MetadataModel model)
        {
            var tempFi = new FileInfo(Path.Combine(Path.GetTempPath(), model.TempId.ToString()));

            if (!tempFi.Exists)
                throw new Exception("file not found");
            else
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "attaches", model.TempId.ToString());
                var destFi = new FileInfo(path);

                if (destFi.Directory != null && !destFi.Directory.Exists)
                    destFi.Directory.Create();

                System.IO.File.Copy(tempFi.FullName, path, true);

                var user = await _userService.GetUserById(post.CreatorId);

                var postContent = new PostContent
                {
                    PostId = post.Id,
                    Post = post,
                    Author = user,
                    MimeType = model.MimeType,
                    FilePath = path,
                    Name = model.Name,
                    Size = model.Size
                };

                if (post.PostContent == null)
                    post.PostContent = new List<PostContent>();

                post.PostContent.Add(postContent);
                await _dataContext.SaveChangesAsync();
            }
        }

        public async Task WriteComment(Guid userId, Guid postId, string text)
        {
            if (text == null)
                throw new Exception("text of comment can not be empty");

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                AuthorId = userId,
                Text = text,
                CreatingDate = DateTimeOffset.UtcNow
            };

            await _dataContext.Comments.AddAsync(comment);
            await _dataContext.SaveChangesAsync();
        }

        public async Task<Post> GetPostById(Guid id)
        {
            var post = await _dataContext
                .Posts
                .Include(e => e.PostContent)
                .Include(e => e.Comments)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (post == null)
                throw new Exception("post not found");

            return post;
        }

        public async Task<List<Post>> GetPosts(int skip, int take)
        {
            return await _dataContext
                .Posts
                .Include(e => e.PostContent)
                .Take(take)
                .Skip(skip)
                .ToListAsync();
        }
    }
}
