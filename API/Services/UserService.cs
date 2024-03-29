﻿using API.Configs;
using API.Models;
using AutoMapper;
using Common;
using DAL;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace API.Services
{
    public class UserService : IDisposable
    {
        private readonly IMapper _mapper;
        private readonly DataContext _dataContext;
        private readonly AuthConfig _config;

        public UserService(IMapper mapper, DataContext context, IOptions<AuthConfig> config)
        {
            _mapper = mapper;
            _dataContext = context;
            _config = config.Value;
        }

        public async Task<bool> CheckUserExist(string email)
        {
            return await _dataContext.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());
        }

        public async Task<Guid> CreateUser(CreateUserModel model)
        {
            var user = _mapper.Map<DAL.Entities.User>(model);
            var temp = await _dataContext.Users.AddAsync(user);
            await _dataContext.SaveChangesAsync();

            return temp.Entity.Id;
        }

        public async Task DeleteUser(Guid id)
        {
            var dbUser = await GetUserById(id);
            if (dbUser != null)
            {
                _dataContext.Users.Remove(dbUser);
                await _dataContext.SaveChangesAsync();
            }
        }

        public async Task<List<UserWithAvatarModel>> GetUsers()
            => await _dataContext.Users
            .Include(x => x.Avatar)
            .Include(x => x.Posts)
            .Select(x => _mapper.Map<UserWithAvatarModel>(x))
            .ToListAsync();

        public async Task<User> GetUserById(Guid id)
        {
            var user = await _dataContext.Users
                .Include(x => x.Avatar)
                .Include(x => x.Posts)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
                throw new Exception("user not found");
            return user;
        }

        public async Task<UserWithAvatarModel> GetUser(Guid id)
        {
            var user = await GetUserById(id);

            return _mapper.Map<UserWithAvatarModel>(user);
        }

        private async Task<User> GetUserByCredention(string login, string password)
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

        private TokenModel GenerateToken(UserSession session)
        {
            var dtNow = DateTime.Now;

            if (session.User == null)
                throw new Exception("wtf");

            var jwt = new JwtSecurityToken(
                issuer: _config.Issuer,
                audience: _config.Audience,
                notBefore: dtNow,
                claims: new Claim[]
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, session.User.Name),
                    new Claim("sessionId",session.Id.ToString()),
                    new Claim("id",session.User.Id.ToString())
                },
                expires: DateTime.Now.AddMinutes(_config.LifeTime),
                signingCredentials: new SigningCredentials(_config.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256)
                );

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var refresh = new JwtSecurityToken(
                notBefore: dtNow,
                claims: new Claim[]
                {
                    new Claim("refreshToken", session.RefreshToken.ToString())
                },
                expires: DateTime.Now.AddHours(_config.LifeTime),
                signingCredentials: new SigningCredentials(_config.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256)
                );

            var encodedRefresh = new JwtSecurityTokenHandler().WriteToken(refresh);

            return new TokenModel(encodedJwt, encodedRefresh);
        }

        public async Task<TokenModel> GetToken(string login, string password)
        {
            var user = await GetUserByCredention(login, password);
            var session = await _dataContext.UserSessions.AddAsync(new DAL.Entities.UserSession
            {
                User = user,
                RefreshToken = Guid.NewGuid(),
                CreationTime = DateTime.UtcNow,
                Id = Guid.NewGuid()
            });
            await _dataContext.SaveChangesAsync();
            return GenerateToken(session.Entity);
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

            if (principal.Claims.FirstOrDefault(x => x.Type == "refreshToken")?.Value is String refreshIdString
                && Guid.TryParse(refreshIdString, out var refreshId))
            {
                var session = await GetSessionByRefreshToken(refreshId);
                if (!session.IsActive)
                {
                    throw new Exception("session is not active");
                }
                session.RefreshToken = Guid.NewGuid();
                await _dataContext.SaveChangesAsync();
                return GenerateToken(session);
            }
            else
            {
                throw new SecurityTokenException("invalid token");
            }
        }

        public async Task<UserSession> GetSessionById(Guid id)
        {
            var session = await _dataContext.UserSessions.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);

            if (session==null)
            {
                throw new Exception("session is not found");
            }
            return session;
        }

        public async Task<UserSession> GetSessionByRefreshToken(Guid refreshToken)
        {
            var session = await _dataContext.UserSessions.Include(x => x.User).FirstOrDefaultAsync(x 
                => x.RefreshToken == refreshToken);

            if (session == null)
            {
                throw new Exception("session is not found");
            }
            return session;
        }

        public async Task AddAvatarToUser(Guid userId, MetadataModel meta, string filePath)
        {
            var user = await GetUserById(userId);

            if (user != null)
            {
                var avatar = new Avatar
                {
                    Author = user,
                    MimeType = meta.MimeType,
                    FilePath = filePath,
                    Name = meta.Name,
                    Size = meta.Size
                };
                user.Avatar = avatar;

                await _dataContext.SaveChangesAsync();
            }
        }

        public async Task<AttachModel> GetUserAvatar(Guid userId)
        {
            var user = await GetUserById(userId);
            var atach = _mapper.Map<AttachModel>(user.Avatar);
            return atach;
        }

        public async Task Subscribe(Guid targetId, Guid subId)
        {
            var targetUser = await _dataContext.Users.FirstOrDefaultAsync(x => x.Id == targetId);

            if (targetUser == default)
                throw new Exception("targetUser not found");

            if (targetId == subId)
                throw new Exception("you cant subscripe on yourself");

            var sub = await _dataContext.Subs.FirstOrDefaultAsync(x 
                => x.TargetId == targetId && x.SubscriberId == subId);

            if (sub!=default)
                _dataContext.Subs.Remove(sub);
            else
            {
                sub = new Subscribe
                {
                    TargetId = targetId,
                    SubscriberId = subId
                };

                await _dataContext.Subs.AddAsync(sub);
            }

            await _dataContext.SaveChangesAsync();
        }

        public async Task<bool> IsSubscribed(Guid targetId, Guid subId)
        {
            var target = await _dataContext.Users.FirstOrDefaultAsync(x => x.Id == targetId);
            var sub = await _dataContext.Users.FirstOrDefaultAsync(x => x.Id == subId);

            if (target == null)
                throw new Exception("target user not found");
            if (sub == null)
                throw new Exception("sub user not found");

            var subsscribe_line = await _dataContext.Subs.FirstOrDefaultAsync(x
                => x.TargetId ==targetId && x.SubscriberId == subId);

            return subsscribe_line != null;
        }

        // на кого подписан пользователь
        public async Task<List<UserWithAvatarModel>> GetSubscribsions(Guid userId)
        {
            var user = await GetUserById(userId);

            if (user == default)
                throw new Exception("you are not autorized");

            var subs = await _dataContext.Subs
                .Include(x => x.Target)
                .Include(x => x.Target.Avatar)
                .Include(x => x.Target.Posts)
                .Where(x => x.SubscriberId == userId)
                .ToListAsync();

            var users = new List<UserWithAvatarModel>();

            foreach (var sub in subs)
            {
                users.Add(_mapper.Map<UserWithAvatarModel>(sub.Target));
            }

            return users;
        }

        // кто подписан на пользователя
        public async Task<List<UserWithAvatarModel>> GetSubscribers(Guid userId)
        {
            var user = await GetUserById(userId);

            if (user == default)
                throw new Exception("you are not autorized");

            var subs = await _dataContext.Subs
                .Include(x => x.Subscriber)
                .Include(x => x.Subscriber.Avatar)
                .Include(x => x.Subscriber.Posts)
                .Where(x => x.TargetId == userId)
                .ToListAsync();

            var users = new List<UserWithAvatarModel>();

            foreach (var sub in subs)
            {
                users.Add(_mapper.Map<UserWithAvatarModel>(sub.Subscriber));
            }

            return users;
        }

        public void Dispose() => _dataContext.Dispose();
    }
}
