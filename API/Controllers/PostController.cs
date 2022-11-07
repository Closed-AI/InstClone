using API.Models;
using API.Services;
using AutoMapper;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly PostService _postService;
        private readonly IMapper _mapper;

        public PostController(PostService postService, IMapper mapper)
        {
            _postService = postService;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize]
        public async Task CreatePost(string? description)
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;

            if (Guid.TryParse(userIdString, out var userId))
            {
                await _postService.CreatePost(userId, description);
            }
            else
                throw new Exception("you are not authorized");
        }

        [HttpPost]
        [Authorize]
        public async Task AddContentToPost(AddContentRequestModel model)
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;

            if (Guid.TryParse(userIdString, out var userId))
            {
                var post = await _postService.GetPostById(model.PostId);

                if (post.CreatorId != userId)
                    throw new Exception("you can not edit not your post");

                var tempFi = new FileInfo(Path.Combine(Path.GetTempPath(), model.Meta.TempId.ToString()));

                if (!tempFi.Exists)
                    throw new Exception("file not found");
                else
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "attaches", model.Meta.TempId.ToString());
                    var destFi = new FileInfo(path);

                    if (destFi.Directory != null && !destFi.Directory.Exists)
                        destFi.Directory.Create();

                    System.IO.File.Copy(tempFi.FullName, path, true);

                    await _postService.AddContentToPost(post.Id, userId, model.Meta, path);
                }
            }
            else
                throw new Exception("you are not authorized");
        }

        [HttpPost]
        [Authorize]
        public async Task WriteComment(WriteCommentRequestModel model)
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;

            if (Guid.TryParse(userIdString, out var userId))
            {
                await _postService.WriteComment(userId, model.PostId, model.Text);
            }
            else
                throw new Exception("you are not authorized");
        }

        [HttpGet]
        public async Task<List<CommentModel>> ShowPostComments(Guid postId)
        {
            var post = await _postService.GetPostById(postId);
            return _mapper.Map<List<CommentModel>>(post.Comments);
        }

        [HttpGet]
        public async Task<PostModel> GetPost(Guid id)
        {
            // аттачи поста можно получить с помощью метода
            // AttachController  GetAttach  и  GetAttachById
            var post = await _postService.GetPostById(id);

            if (post == null)
                throw new Exception("post not found");

            var content = new List<AttachModel>();

            if (post.PostContent != null)
            {
                foreach (var el in post.PostContent)
                {
                    if (el != null)
                        content.Add(_mapper.Map<AttachModel>(el));
                }
            }

            var model = new PostModel
            {
                Id = post.Id,
                Description = post.Description,
                CreatingDate = post.CreatingDate,
                Content = content
            };

            return model;

            // чёртов мапер мог заменить строки 80 - 97
            // строка ниже не может смапить  Post.PostContent в PostModel.AttachModel
            //return _mapper.Map<PostModel>(posts);
        }

        [HttpGet]
        public async Task<List<PostModel>> GetPosts()
        {
            var posts = await _postService.GetPosts();

            if (posts == null)
                return new List<PostModel>();

            var postModels = new List<PostModel>();

            foreach(var item in posts)
            {
                postModels.Add(await GetPost(item.Id));
            }

            return postModels;

            // чёртов мапер мог заменить строки 80 - 97
            // строка ниже не может смапить  Post.ICollection<PostContent> в PostModel.List<AttachModel>
            //return _mapper.Map<List<PostModel>>(posts);
        }
    }
}
