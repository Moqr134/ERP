using Infrastructure.Logger;
using Infrastructure.ORM;

namespace ERP_API.Infrastructure.Services
{
    public class MasterService
    {
        public readonly DBContext Context;
        public MasterService(DBContext context)
        {
            Context = context;
        }
    }
}
