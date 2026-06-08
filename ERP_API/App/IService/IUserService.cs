using ERP_API.Domin.UsersEntity;

namespace ERP_API.App.IService
{
    public interface IUserService
    {
        public Users GetUser(int id);
        public Task<Users> CheckUser(string Name);
        public Task<Users> CheckUserExsist(string Name);
        public Task<List<Users>> GetUserList(int pageIndex = 1, int pageSize = 10);
        public Task<List<Users>> GetUser(string Email);
        public Task<List<Users>> GetUserByName(string Name);
    }
}
