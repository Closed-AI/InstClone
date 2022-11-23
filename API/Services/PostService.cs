using API.Models;
using DAL.Entities;
using DAL;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace API.Services
{
    public class PostService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _dataContext;

        public PostService(IMapper mapper, DataContext dataContext)
        {
            _mapper = mapper;
            _dataContext = dataContext;
        }

        public async Task CreatePost(CreatePostRequest request)
        {
            var model = _mapper.Map<CreatePostModel>(request);

            model.Contents?.ForEach(e =>
            {
                e.AuthorId = model.AuthorId;
                e.FilePath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "attaches",
                    e.TempId.ToString());

                var tempFI = new FileInfo(Path.Combine(Path.GetTempPath(), e.TempId.ToString()));

                if (tempFI.Exists)
                {
                    var destFI = new FileInfo(e.FilePath);

                    if (destFI.Directory != null && !destFI.Directory.Exists)
                        destFI.Directory.Create();

                    File.Move(tempFI.FullName, e.FilePath, true);
                }
            });

            var dbEntity = _mapper.Map<Post>(model);
            await _dataContext.Posts.AddAsync(dbEntity);
            await _dataContext.SaveChangesAsync();
        }

        public async Task LikePost(Guid postId, Guid userId)
        {
            var user = await _dataContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
            var post = await _dataContext.Posts.FirstOrDefaultAsync(x => x.Id == postId);

            if (post == null)
                throw new Exception("Post does not exist");

            if (user == null)
                throw new Exception("Post does not exist");

            var temp = await _dataContext.PostLikes.FirstOrDefaultAsync(x 
                => x.PostId == postId && x.UserId == userId);

            if (temp != default)
            {
                _dataContext.PostLikes.Remove(temp);
            }
            else
            {
                var like = new PostLike
                {
                    User = user,
                    Comment = post
                };

                await _dataContext.PostLikes.AddAsync(like);
            }

            await _dataContext.SaveChangesAsync();
        }

        public async Task WriteComment(CreateCommentModel model)
        {
            if (model.Text == null)
                throw new Exception("text of comment can not be empty");

            var entity = _mapper.Map<Comment>(model);

            await _dataContext.Comments.AddAsync(entity);
            await _dataContext.SaveChangesAsync();
        }

        public async Task LikeComment(Guid commentId, Guid userId)
        {
            var user = await _dataContext.Users.FirstOrDefaultAsync(x => x.Id == userId);
            var comment = await _dataContext.Comments.FirstOrDefaultAsync(x => x.Id == commentId);

            if (comment == null)
                throw new Exception("Post does not exist");

            if (user == null)
                throw new Exception("Post does not exist");

            var temp = await _dataContext.CommentLikes.FirstOrDefaultAsync(x
                => x.CommentId == commentId && x.UserId == userId);

            if (temp != default)
            {
                _dataContext.CommentLikes.Remove(temp);
            }
            else
            {
                var like = new CommentLike
                {
                    User = user,
                    Comment = comment
                };

                await _dataContext.CommentLikes.AddAsync(like);
            }

            await _dataContext.SaveChangesAsync();
        }

        public async Task<List<CommentModel>> ShowPostComments(Guid postId)
        {
            var entitise = await _dataContext.Comments.Include(x => x.Likes)
                .Where(x => x.PostId == postId).ToListAsync();

            return _mapper.Map<List<CommentModel>>(entitise);
        }

        public async Task<PostModel> GetPostById(Guid id)
        {
            var post = await _dataContext.Posts
                .Include(x => x.Author).ThenInclude(x => x.Avatar)
                .Include(x => x.PostContent).AsNoTracking().OrderByDescending(x => x.CreatingDate)
                .Include(x => x.Comments).OrderByDescending(x => x.CreatingDate)
                .Include(x => x.Likes)
                .Where(x => x.Id == id)
                .Select(x => _mapper.Map<PostModel>(x))
                .FirstOrDefaultAsync();

            if (post == null)
                throw new Exception("post not found");

            return post;
        }

        public async Task<List<PostModel>> GetPosts(int skip, int take)
        {
            var posts = await _dataContext.Posts
                .Include(x => x.Author).ThenInclude(x => x.Avatar)
                .Include(x => x.PostContent).AsNoTracking().OrderByDescending(x => x.CreatingDate)
                .Include(x => x.Comments).ThenInclude(x=>x.Likes)
                .OrderByDescending(x => x.CreatingDate)
                .Include(x => x.Likes)
                .Skip(skip).Take(take)
                .Select(x => _mapper.Map<PostModel>(x))
                .ToListAsync();
            
            return posts;
        }

        public async Task<AttachModel> GetPostContent(Guid postContentId)
        {
            var content = await _dataContext.PostContents
                .FirstOrDefaultAsync(x => x.Id == postContentId);

            return _mapper.Map<AttachModel>(content);
        }
    }
}
