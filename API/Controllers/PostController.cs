using Api.Controllers;
using API.Models;
using API.Services;
using AutoMapper;
using Common;
using Microsoft.AspNetCore.Authorization;
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
        public async Task CreatePost(CreatePostModel model)
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;

            if (Guid.TryParse(userIdString, out var userId))
            {
                await _postService.CreatePost(userId, model);
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
            var post = await _postService.GetPostById(id);

            if (post == null)
                throw new Exception("post not found");
            
            var res = _mapper.Map<PostModel>(post);

            res.Contents = new List<AttachWithLinkModel>();

            if (post.PostContent != null)
                foreach (var item in post.PostContent)
                {
                    var link = Url.Action(
                        nameof(AttachController.GetAttach),
                        nameof(AttachController).CutController(),
                        new { id = item.Id }
                        );

                    if (link != null)
                    {
                        var model = new AttachWithLinkModel
                        {
                            Name = item.Name,
                            MimeType = item.MimeType,
                            Link = link
                        };

                        res.Contents.Add(model);
                    }
                }

            return res;
        }

        [HttpGet]
        public async Task<List<PostModel>> GetPosts(int skip = 0, int take = 10)
        {
            var posts = await _postService.GetPosts(skip, take);

            if (posts == null)
                return new List<PostModel>();

            var postModels = new List<PostModel>();

            foreach(var item in posts)
            {
                postModels.Add(await GetPost(item.Id));
            }

            return postModels;
        }
    }
}
