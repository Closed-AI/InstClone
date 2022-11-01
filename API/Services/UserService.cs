using API.Configs;
using API.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API.Services
{
    public class UserService
    {
        private readonly IMapper _mapper;
        private readonly DAL.DataContext _dataContext;
        private readonly AuthConfig _config;

        public UserService(IMapper mapper, DataContext context, IOptions<AuthConfig> config)
        {
            _mapper = mapper;
            _dataContext = context;
            _config = config.Value;
        }

        public async Task CreateUser(CreateUserModel model)
        {
            var user = _mapper.Map<DAL.Entities.User>(model);
            await _dataContext.Users.AddAsync(user);
            await _dataContext.SaveChangesAsync();
        }

        public async Task<List<UserModel>> GetUsers()
            => await _dataContext.Users.AsNoTracking()
            .ProjectTo<UserModel>(_mapper.ConfigurationProvider).ToListAsync();

        private async Task<DAL.Entities.User> GetUserById(Guid id)
        {
            var user = await _dataContext.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
                throw new Exception("user not found");
            return user;
        }
        public async Task<UserModel> GetUser(Guid id)
        {
            var user = await GetUserById(id);

            return _mapper.Map<UserModel>(user);
        }


        private async Task<DAL.Entities.User> GetUserByCredention(string login, string password)
        {
            var user = await _dataContext
                .Users
                .FirstOrDefaultAsync(e => e.Email.ToLower() == login.ToLower());

            if (user == null)
                throw new Exception("user not found");

            if (!HashHelper.Verify(password, user.PasswordHash))
                throw new Exception("incorrect password");
            
            return user;
        }

        private TokenModel GenerateToken(DAL.Entities.User user)
        {
            var dtNow = DateTime.Now;

            var jwt = new JwtSecurityToken(
                issuer: _config.Issuer,
                audience: _config.Audience,
                notBefore: dtNow,
                claims: new Claim[]
                {
                new Claim(ClaimsIdentity.DefaultNameClaimType,user.Name),
                new Claim("id", user.Id.ToString())
            },
                expires: DateTime.Now.AddMinutes(_config.LifeTime),
                signingCredentials: new SigningCredentials(_config.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
                );

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var refresh = new JwtSecurityToken(
                notBefore: dtNow,
                claims: new Claim[]
                {

                new Claim("id", user.Id.ToString())
            },
                expires: DateTime.Now.AddHours(_config.LifeTime),
                signingCredentials: new SigningCredentials(_config.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
                );

            var encodedRefresh = new JwtSecurityTokenHandler().WriteToken(refresh);

            return new TokenModel(encodedJwt, encodedRefresh);
        }

        public async Task<TokenModel> GetToken(string login, string password)
        {
            var user = await GetUserByCredention(login, password);

            return GenerateToken(user);
        }

        public async Task<TokenModel> GetTokenByRefreshToken(string refreshToken)
        {
            var validParams = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKey = _config.GetSymmetricSecurityKey()
            };
            var principal = new JwtSecurityTokenHandler().ValidateToken(refreshToken, validParams, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtToken
                || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("invalid token");
            }

            if (principal.Claims.FirstOrDefault(x => x.Type == "id")?.Value is String userIdString
                && Guid.TryParse(userIdString, out var userId))
            {
                var user = await GetUserById(userId);
                return GenerateToken(user);
            }
            else
            {
                throw new SecurityTokenException("invalid token");
            }
        }
    }
}
