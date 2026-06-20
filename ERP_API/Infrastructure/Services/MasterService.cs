using AutoMapper;
using Infrastructure.Logger;
using Infrastructure.ORM;

namespace ERP_API.Infrastructure.Services
{
    public class MasterService
    {
        public readonly DBContext _context;

        public readonly IMapper _mapper;

        public MasterService(DBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

    }
}
