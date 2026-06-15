using ERP_API.App.IService;
using ERP_API.Domin.UsersEntity;
using ERP_API.Infrastructure.Services;
using Infrastructure.AppException;
using Infrastructure.ORM;
using Infrastructure.Service;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.App.Service
{
    public class UserService : MasterService, IUserService, IScopped
    {

        public UserService(DBContext context):base(context)
        {
            
        }
        public async Task<Users> CheckUser(string Name)
        {
            var user = await Context.Users.Where(x => x.Username == Name && x.IsRemoved == false).Include(x => x.Permations).FirstOrDefaultAsync();
            if (user == null)
                throw new KeyNotFoundException("المستخدم غير موجود");
            else return user;
        }

        public async Task<Users?> CheckUserExsist(string Name)
        {
            var user = await Context.Users.Where(x => x.Username == Name).FirstOrDefaultAsync();
            return user;
        }

        public Users GetUser(int id)
        {
            var user = Context.Users.Find(id);
            if (user is null)
                throw new KeyNotFoundException(nameof(id));
            else return user;
        }
    }
}
