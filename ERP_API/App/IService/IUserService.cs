using ERP_API.Domin.UsersEntity;

namespace ERP_API.App.IService
{
    public interface IUserService
    {
        public Users GetUser(int id);
        public Task<Users> CheckUser(string Name);
        public Task<Users?> CheckUserExsist(string Name);
    }
}
