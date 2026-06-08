using ERP_API.App.IService;
using ERP_API.Domin.UsersEntity;
using ERP_API.Infrastructure.Services;
using Infrastructure.ORM;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.App.Service
{
    public class UserService : MasterService, IUserService
    {

        public UserService(DBContext context):base(context)
        {
            
        }
        public async Task<Users> CheckUser(string Name)
        {
            try
            {
                return await Context.Users.Where(x => x.Username == Name && x.IsRemoved == false).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await loger.WriteAsync(ex, "UserService => CheckUser");
                throw;
            }
        }

        public async Task<Users> CheckUserExsist(string Name)
        {
            try
            {
                return await Context.Users.Where(x => x.Username == Name).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await loger.WriteAsync(ex, "UserService => CheckUserExsist");
                throw;
            }
        }

        public Users GetUser(int id)
        {
            try
            {
                return Context.Users.Find(id);
            }
            catch (Exception ex)
            {
                loger.Write(ex, "UserService => GetUser");
                throw;
            }
        }

        public async Task<List<Users>> GetUser(string Name)
        {
            try
            {
                return await Context.Users
                    .Where(x => x.Username.Contains(Name) && x.IsRemoved == false && x.Role != "Maneger")
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                loger.Write(ex, "UserService => GetUser");
                throw;
            }
        }

        public async Task<List<Users>> GetUserByName(string Name)
        {
            try
            {
                return await Context.Users
                    .Where(x => x.Username.Contains(Name) && x.IsRemoved == false && x.Role != "Maneger")
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                loger.Write(ex, "UserService => GetUserByName");
                throw;
            }
        }

        public async Task<List<Users>> GetUserList(int pageIndex = 1, int pageSize = 10)
        {
            try
            {
                return await Context.Users
                .Where(u => u.Role != "Maneger")
                .OrderBy(u => u.Id)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                loger.Write(ex, "UserService => GetUserList");
                throw;
            }
        }
    }
}
