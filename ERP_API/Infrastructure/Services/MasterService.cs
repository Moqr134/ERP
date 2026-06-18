using AutoMapper;
using Infrastructure.Logger;
using Infrastructure.ORM;

namespace ERP_API.Infrastructure.Services
{
    public class MasterService
    {
        public readonly DBContext Context;

        public readonly IMapper _mapper;

        public MasterService(DBContext context, IMapper mapper)
        {
            Context = context;
            _mapper = mapper;
        }
    }
}
