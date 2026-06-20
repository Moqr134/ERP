using AutoMapper;
using ERP_API.App.IService;
using ERP_API.Domin.PermartionEntity;
using ERP_API.Domin.RoleEntity;
using ERP_API.Domin.UsersEntity;
using ERP_API.Infrastructure.Services;
using ERPDto.PaigingDto;
using ERPDto.UserDto;
using Infrastructure.AppException;
using Infrastructure.ORM;
using Infrastructure.Service;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.App.Service
{
    public class UserService : MasterService, IUserService, IScopped
    {
        public UserService(DBContext context, IMapper mapper):base(context, mapper)
        {
            
        }
        public async Task<Users?> CheckUserExsist(string Name)
        {
            var user = await _context.Users.Where(x => x.Username == Name).FirstOrDefaultAsync();
            return user;
        }
        public Task<List<UserOut>> GetUsers(PageDto pageDto)
        {
            var users = _context.Users.Where(x => x.IsRemoved == false)
                .Select(x => new UserOut
                {
                    Id = x.Id,
                    Username = x.Username,
                    Email = x.Email,
                })
                .Skip((pageDto.PageIndex - 1) * pageDto.PageSize)
                .Take(pageDto.PageSize)
                .ToListAsync();
            return users;
        }
        public async Task<UserOut> CheckUser(string Name)
        {
            var user = await _context.Users.Where(x => x.Username == Name && x.IsRemoved == false)
                .Select(x => new UserOut
                {
                    Id = x.Id,
                    Username = x.Username,
                    Email = x.Email,
                })
                .FirstOrDefaultAsync();
            if (user == null)
                throw new KeyNotFoundException("المستخدم غير موجود");
            else return user;
        }
        public UserOut GetUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user is null or { IsRemoved: true })
                throw new KeyNotFoundException(nameof(id));
            else return _mapper.Map<Users, UserOut>(user);
        }
        public Task<Role?> GetUserRole(int userId)
        {
            var role = _context.UserRoles
                .Where(x => x.UserId == userId)
                .Include(x => x.Role)
                .Select(x => x.Role)
                .FirstOrDefaultAsync();
            if (role == null)
                throw new KeyNotFoundException("الدور غير موجود");
            else return role;
        }
        public async Task<List<Permission>> GetUserPermissions(int userId)
        {
            List<Permission> permissions = await _context.UserPermissions
                .Where(x => x.UserId == userId)
                .Include(x => x.Permission)
                .Select(x => x.Permission)
                .ToListAsync();
            if (permissions == null)
                throw new KeyNotFoundException("الصلاحيات غير موجودة");
            else return permissions;
        }

        public async Task<Role> GetRole(string RoleName)
        {
            var role = await _context.Roles.Where(x => x.Name == RoleName && x.IsRemoved == false).FirstOrDefaultAsync();
            if (role == null)
                throw new KeyNotFoundException("الدور غير موجود");
            else return role;
        }

        public async Task<List<Role>> GetUserRoles(int userId)
        {
            var roles = await _context.UserRoles
                .Where(x => x.UserId == userId)
                .Include(x => x.Role)
                .Select(x => x.Role)
                .ToListAsync();
            if (roles == null)
                throw new KeyNotFoundException("الدوريات غير موجودة");
            else return roles;
        }

        public async Task<List<Permission>> GetRolePermissions(int Id)
        {
            List<Permission> permissions = await _context.RolePermissions
                .Where(x => x.RoleId == Id)
                .Include(x => x.Permission)
                .Select(x => x.Permission)
                .ToListAsync();
            if (permissions == null)
                throw new KeyNotFoundException("الصلاحيات غير موجودة");
            else return permissions;
        }

        public async Task<Users?> GetFullUser(string Name)
        {
            Users? users = await _context.Users.Where(x => x.Username == Name && !x.IsRemoved)
                .Include(x => x.UserRoles)
                .FirstOrDefaultAsync();
            if (users is null)
                throw new KeyNotFoundException("المستخدم غير موجود");
            else return users;
        }

        public async Task<Users?> GetUserByRefreshToken(string refreshToken)
        {
            Users? users = await _context.Users.Where(x => x.RefreshToken == refreshToken && !x.IsRemoved)
                .Include(x => x.UserRoles)
                .FirstOrDefaultAsync();
            if (users is null)
                throw new KeyNotFoundException("المستخدم غير موجود");
            else return users;
        }
    }
}
