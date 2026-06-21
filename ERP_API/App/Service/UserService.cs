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
        public async Task<List<UserOut>> GetUsers(PageDto pageDto)
        {
            List<UserOut> users = await _context.Users.Where(x => x.IsRemoved == false)
                
                .Skip((pageDto.PageIndex - 1) * pageDto.PageSize)
                .Take(pageDto.PageSize)
                .Select(x => new UserOut
                {
                    Id = x.Id,
                    Username = x.Username,
                    Email = x.Email,
                })
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
        public async Task<List<Permission>> GetUserPermissions(int userId,int roleId)
        {
            List<UserPermissions> permissions = await _context.UserPermissions
                .Where(x => x.UserId == userId)
                .Include(x => x.Permission)
                .ToListAsync();
            List<Permission> RolePermissions =await GetRolePermissions(roleId);
            if (permissions == null || RolePermissions == null)
                throw new KeyNotFoundException("الصلاحيات غير موجودة");
            List<Permission> NotAllowedPermissions = permissions.Where(x=>x.IsAllowed == false).Select(x => x.Permission).ToList();
            List<Permission> AllowedPermissions = permissions.Where(x=>x.IsAllowed == true).Select(x => x.Permission).ToList();
            foreach (var item in NotAllowedPermissions)
            {
                RolePermissions.RemoveAll(x => x.Id == item.Id);
            }
            AllowedPermissions.AddRange(RolePermissions);
            return AllowedPermissions;
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

        public async Task UpdateUser(UpdateUserModel model, int updateUserId)
        {
            Users users = GetUserById(model.Id);
            if(model.Username != null)
                users.Username = model.Username;
            if (model.Email != null)
                users.Email = model.Email;
            users.UpdateDate = DateTime.UtcNow.AddHours(3);
            users.UpdateUserId = updateUserId;
            _context.Entry(users).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUser(int userId, int removeUserId)
        {
            Users users = GetUserById(userId);
            users.IsRemoved = true;
            users.RemoveDate = DateTime.UtcNow.AddHours(3);
            users.RemoveUserId = removeUserId;
            _context.Entry(users).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public Users GetUserById(int Id)
        {
            Users? user = _context.Users.Find(Id);
            if (user is null || user.IsRemoved)
                throw new KeyNotFoundException("المستخدم غير موجود");
            else return user;
        }
    }
}
