using Api.Controllers;
using API.Models;
using API.Services;
using AutoMapper;
using Common.Consts;
using Common.Extensions;
using Common.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Api")]
    public class PostController : ControllerBase
    {
        private readonly PostService _postService;
        private readonly IMapper _mapper;

        public PostController(PostService postService, IMapper mapper, LinkGeneratorService links)
        {
            _postService = postService;
            _mapper = mapper;

            links.LinkContentGenerator = x => Url.ControllerAction<AttachController>
            (
                nameof(AttachController.GetPostContent), new
                {
                    postContentId = x.Id,
                }
            );
            links.LinkAvatarGenerator = x => Url.ControllerAction<AttachController>
            (
                nameof(AttachController.GetUserAvatar), new
                {
                    userId = x.Id,
                }
            );
        }

        [HttpPost]
        [Authorize]
        public async Task CreatePost(CreatePostRequest request)
        {
            if (!request.AuthorId.HasValue)
            {
                var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
                if (userId == default)
                    throw new Exception("not authorize");
                request.AuthorId = userId;
            }
            await _postService.CreatePost(request);
        }

        [HttpPost]
        [Authorize]
        public async Task WriteComment(CreateCommentRequest request)
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;

            if (Guid.TryParse(userIdString, out var userId))
            {
                var model = _mapper.Map<CreateCommentModel>(request);
                model.AuthorId = userId;

                await _postService.WriteComment(model);
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
        public async Task<PostModel> GetPostById(Guid id)
            => await _postService.GetPostById(id);

        [HttpGet]
        public async Task<List<PostModel>> GetPosts(int skip = 0, int take = 10)
            => await _postService.GetPosts(skip, take);
    }
}
