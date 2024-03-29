﻿using Api.Controllers;
using API.Models;
using API.Services;
using Common.Consts;
using Common.Extentions;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Api")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService, LinkGeneratorService links)
        {
            _userService = userService;

            links.LinkAvatarGenerator = x =>
            Url.ControllerAction<AttachController>(nameof(AttachController.GetUserAvatar), new
            {
                userId = x.Id,
            });
        }

        [HttpGet]
        [Authorize]
        public async Task DeleteUser(Guid userID)
        {
            var curUserId = User.GetClaimValue<Guid>(ClaimNames.Id);

            if (userID != curUserId)
                throw new Exception("you cant delete diffrent user");

            await _userService.DeleteUser(userID);
        }

        [HttpGet]
        [Authorize]
        public async Task<List<UserWithAvatarModel>> GetUsers() => await _userService.GetUsers();

        [HttpGet]
        [Authorize]
        public async Task<UserModel> GetCurrentUser()
        {
            var idString = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

            if (Guid.TryParse(idString, out Guid userId))
            {
                return await _userService.GetUser(userId);
            }
            else
                throw new Exception("you are not authorized");
        }

        [HttpGet]
        public async Task<UserWithAvatarModel> GetUserById(Guid userId) 
            => await _userService.GetUser(userId);

        [HttpPost]
        [Authorize]
        public async Task AddAvatarToUser(MetadataModel model)
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;

            if (Guid.TryParse(userIdString, out var userId))
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

                    await _userService.AddAvatarToUser(userId, model, path);
                }
            }
            else
                throw new Exception("you are not authorized");
        }

        [HttpPost]
        [Authorize]
        public async Task Subscribe(Guid targetId)
        {
            // если не подписан - подписывается
            // если подписан - отписывается
            var idString = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;

            if (Guid.TryParse(idString, out Guid userId))
            {
                await _userService.Subscribe(targetId, userId);
            }
            else
                throw new Exception("you are not authorized");
        }

        [HttpGet]
        [Authorize]
        public async Task<bool> IsSubscribed(Guid targetId, Guid subId)=>
            await _userService.IsSubscribed(targetId, subId);

        [HttpGet]
        [Authorize]
        public async Task<List<UserWithAvatarModel>> GetSubscribsions(Guid userId)
            => await _userService.GetSubscribsions(userId);

        [HttpGet]
        [Authorize]
        public async Task<List<UserWithAvatarModel>> GetSubscribers(Guid userId)
            => await _userService.GetSubscribers(userId);
    }
}
