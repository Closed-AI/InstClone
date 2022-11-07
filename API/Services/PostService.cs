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

        public async Task CreatePost(Guid creatorId, string? description)
        {
            var post = new Post
            {
                Id = Guid.NewGuid(),
                Description = description,
                CreatingDate = DateTime.UtcNow,
                CreatorId = creatorId
            };

            await _dataContext.Posts.AddAsync(post);
            await _dataContext.SaveChangesAsync();
        }

        public async Task AddContentToPost(Guid postId, Guid userID, MetadataModel meta, string filePath)
        {
            var post = await GetPostById(postId);
            var user = await _userService.GetUserById(userID);

            if (post != null && user != null)
            {
                var postContent = new PostContent
                {
                    PostId = post.Id,
                    Author = user,
                    MimeType = meta.MimeType,
                    FilePath = filePath,
                    Name = meta.Name,
                    Size = meta.Size
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

        public async Task<List<Post>> GetPosts()
        {
            return await _dataContext.Posts.Include(e => e.PostContent).ToListAsync();
        }
    }
}
