using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task CreateUser(CreateUserModel model) => await _userService.CreateUser(model);
        
        [HttpGet]
        [Authorize]
        public async Task<List<UserModel>> GetUsers() => await _userService.GetUsers();

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
    }
}
