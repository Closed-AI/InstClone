using API.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DAL;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class UserService
    {
        private readonly IMapper _mapper;
        private readonly DAL.DataContext _dataContext;

        public UserService(IMapper mapper, DataContext dataContext)
        {
            _mapper = mapper;
            _dataContext = dataContext;
        }

        public async Task CreateUser(CreateUserModel model)
        {
            var user = _mapper.Map<DAL.Entitise.User>(model);
            await _dataContext.Users.AddAsync(user);
            await _dataContext.SaveChangesAsync();
        }

        public async Task<List<UserModel>> GetUsers()
            => await _dataContext.Users.AsNoTracking()
            .ProjectTo<UserModel>(_mapper.ConfigurationProvider).ToListAsync();
    }
}
