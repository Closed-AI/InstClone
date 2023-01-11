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
        public async Task DeletePost(Guid postId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);

            await _postService.DeletePost(postId, userId);
        }

        [HttpPost]
        [Authorize]
        public async Task LikePost(Guid postId)
        {
            // лайк и отмена лайка производятся вызовом одного метода,
            // если пользователь ещё не лайкнул данный пост - поставится лайк,
            // если лайкнул - лайк пропадёт

            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;

            if (Guid.TryParse(userIdString, out var userId))
            {
                await _postService.LikePost(postId, userId);
            }
            else
                throw new Exception("you are not authorized");
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

        [HttpPost]
        [Authorize]
        public async Task LikeComment(Guid commentId)
        {
            // лайк и отмена лайка производятся вызовом одного метода,
            // если пользователь ещё не лайкнул данный пост - поставится лайк,
            // если лайкнул - лайк пропадёт

            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;

            if (Guid.TryParse(userIdString, out var userId))
            {
                await _postService.LikeComment(commentId, userId);
            }
            else
                throw new Exception("you are not authorized");
        }

        [HttpGet]
        [Authorize]
        public async Task<List<CommentModel>> ShowPostComments(Guid postId)
            => await _postService.ShowPostComments(postId);
        
        [HttpGet]
        [Authorize]
        public async Task<PostModel> GetPostById(Guid id)
            => await _postService.GetPostById(id);

        [HttpGet]
        [Authorize]
        public async Task<List<PostModel>> GetUserPosts(Guid userId, int skip = 0, int take = 10)
            => await _postService.GetUserPosts(userId, skip, take);

        [HttpGet]
        [Authorize]
        public async Task<List<PostModel>> GetLikedPosts(Guid userId, int skip = 0, int take = 10)
            => await _postService.GetLikedPosts(userId, skip, take);

        [HttpGet]
        [Authorize]
        public async Task<List<PostModel>> GetPosts(int skip = 0, int take = 10)
            => await _postService.GetPosts(skip, take);
    }
}
