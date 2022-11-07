using API.Models;
using AutoMapper;
using DAL;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class AttachService
    {
        private readonly DAL.DataContext _dataContext;
        private readonly IMapper _mapper;

        public AttachService(DataContext dataContext, IMapper mapper)
        {
            _dataContext = dataContext;
            _mapper = mapper;
        }

        public async Task<AttachModel> GetAttachById(long id)
        {
            var res = await _dataContext
                .Attaches
                .FirstOrDefaultAsync(e => e.Id == id);

            if (res == null)
                throw new Exception("attach not found");

            return _mapper.Map<AttachModel>(res);
        }
    }
}
