using AutoMapper;
using ERP_API.App.IService;
using ERP_API.Domin.PermartionEntity;
using ERP_API.Domin.RoleEntity;
using ERP_API.Domin.UsersEntity;
using ERP_API.Infrastructure.Services;
using ERPDto.PaigingDto;
using ERPDto.UserDto;
using Infrastructure.AppException;
using Infrastructure.JWT;
using Infrastructure.ORM;
using Infrastructure.PassowdHashing;
using Infrastructure.Service;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.App.Service
{
    public class UserService : MasterService, IUserService, IScopped
    {
        private readonly PasswordHashing _passwordHashing = new();

        public UserService(DBContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<Users?> CheckUserExsist(string Name)
        {
            return await _context.Users.Where(x => x.Username == Name).FirstOrDefaultAsync();
        }

        public async Task<List<UserOut>> GetUsers(PageDto pageDto)
        {
            var response = await GetUsersPaged(pageDto);
            return response.Items.Select(u => new UserOut
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email
            }).ToList();
        }

        public async Task<UsersListResponse> GetUsersPaged(PageDto pageDto)
        {
            if (pageDto.PageIndex < 1) pageDto.PageIndex = 1;
            if (pageDto.PageSize < 1) pageDto.PageSize = 10;
            if (pageDto.PageSize > 100) pageDto.PageSize = 100;

            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(pageDto.SearchTerm))
            {
                var term = pageDto.SearchTerm.Trim();
                query = query.Where(x => x.Username.Contains(term) || x.Email.Contains(term));
            }

            var total = await query.CountAsync();
            var users = await query
                .OrderByDescending(x => x.Id)
                .Skip((pageDto.PageIndex - 1) * pageDto.PageSize)
                .Take(pageDto.PageSize)
                .Select(x => new UserDetailDto
                {
                    Id = x.Id,
                    Username = x.Username,
                    Email = x.Email,
                    IsActive = x.IsActive,
                    IsOnline = x.IsOnline,
                    LastLogin = x.LastLogin,
                    CreateDate = x.CreateDate,
                    RoleIds = x.UserRoles.Select(r => r.RoleId).ToList(),
                    RoleNames = x.UserRoles.Select(r => r.Role.Name).ToList()
                })
                .ToListAsync();

            return new UsersListResponse
            {
                Items = users,
                TotalCount = total,
                PageIndex = pageDto.PageIndex,
                PageSize = pageDto.PageSize,
                PageCount = (int)Math.Ceiling(total / (double)pageDto.PageSize)
            };
        }

        public async Task<UserDetailDto> GetUserDetail(int id)
        {
            var user = await _context.Users
                .Where(x => x.Id == id)
                .Select(x => new UserDetailDto
                {
                    Id = x.Id,
                    Username = x.Username,
                    Email = x.Email,
                    IsActive = x.IsActive,
                    IsOnline = x.IsOnline,
                    LastLogin = x.LastLogin,
                    CreateDate = x.CreateDate,
                    RoleIds = x.UserRoles.Select(r => r.RoleId).ToList(),
                    RoleNames = x.UserRoles.Select(r => r.Role.Name).ToList()
                })
                .FirstOrDefaultAsync();

            if (user == null)
                throw new KeyNotFoundException("المستخدم غير موجود");
            return user;
        }

        public async Task<UsersInfo> GetUsersInfo()
        {
            return new UsersInfo
            {
                TotalUsers = await _context.Users.CountAsync(),
                ActiveUsers = await _context.Users.CountAsync(x => x.IsActive),
                InactiveUsers = await _context.Users.CountAsync(x => !x.IsActive),
                OnlineUsers = await _context.Users.CountAsync(x => x.IsOnline)
            };
        }

        public async Task CreateUser(CreateUserModel model, int createUserId)
        {
            var exists = await CheckUserExsist(model.Username);
            if (exists != null)
                throw new DuplicateException("المستخدم موجود مسبقا");

            var emailExists = await _context.Users.AnyAsync(x => x.Email == model.Email);
            if (emailExists)
                throw new DuplicateException("البريد الإلكتروني مستخدم مسبقاً");

            var role = await GetRole(model.RoleId);
            var user = new Users
            {
                Username = model.Username,
                HashPassword = _passwordHashing.HashPassword(model.Password),
                Email = model.Email,
                IsActive = model.IsActive,
                IsRemoved = false,
                IsOnline = false,
                CreateDate = DateTime.UtcNow.AddHours(3),
                CreateUserId = createUserId,
                UserRoles = new List<UserRoles>
                {
                    new UserRoles { RoleId = role.Id }
                }
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task SetUserActive(int userId, bool isActive, int updateUserId)
        {
            if (userId == updateUserId && !isActive)
                throw new InvalidOperationException("لا يمكنك تعطيل حسابك الحالي");

            var user = GetUserById(userId);
            user.IsActive = isActive;
            user.UpdateDate = DateTime.UtcNow.AddHours(3);
            user.UpdateUserId = updateUserId;
            if (!isActive)
            {
                user.IsOnline = false;
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                user.Token = null;
            }
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task AssignUserRoles(int userId, List<int> roleIds, int updateUserId)
        {
            if (roleIds == null || roleIds.Count == 0)
                throw new ArgumentException("يجب اختيار دور واحد على الأقل");

            var user = GetUserById(userId);
            var distinctRoleIds = roleIds.Distinct().ToList();
            foreach (var roleId in distinctRoleIds)
            {
                _ = await GetRole(roleId);
            }

            var existing = await _context.UserRoles.Where(x => x.UserId == userId).ToListAsync();
            _context.UserRoles.RemoveRange(existing);
            await _context.UserRoles.AddRangeAsync(distinctRoleIds.Select(roleId => new UserRoles
            {
                UserId = userId,
                RoleId = roleId
            }));

            user.UpdateDate = DateTime.UtcNow.AddHours(3);
            user.UpdateUserId = updateUserId;
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task ChangePassword(int userId, string newPassword, int updateUserId)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 5)
                throw new ArgumentException("كلمة المرور يجب أن تكون 5 أحرف على الأقل");

            var user = GetUserById(userId);
            user.HashPassword = _passwordHashing.HashPassword(newPassword);
            user.UpdateDate = DateTime.UtcNow.AddHours(3);
            user.UpdateUserId = updateUserId;
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            user.Token = null;
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task ChangeMyPassword(int userId, string currentPassword, string newPassword)
        {
            var user = GetUserById(userId);
            if (!_passwordHashing.VerifyPassword(currentPassword, user.HashPassword))
                throw new UnauthorizedAccessException("كلمة المرور الحالية غير صحيحة");

            await ChangePassword(userId, newPassword, userId);
        }

        public async Task<List<UserPermissionViewDto>> GetUserPermissionsView(int userId)
        {
            _ = GetUserById(userId);
            var roleIds = await _context.UserRoles.Where(x => x.UserId == userId).Select(x => x.RoleId).ToListAsync();
            var rolePermissionIds = await _context.RolePermissions
                .Where(x => roleIds.Contains(x.RoleId))
                .Select(x => x.PermissionId)
                .Distinct()
                .ToListAsync();

            var overrides = await _context.UserPermissions
                .Where(x => x.UserId == userId)
                .ToDictionaryAsync(x => x.PermissionId, x => x.IsAllowed);

            var allPermissions = await _context.Permissions.OrderBy(p => p.Name).ToListAsync();
            return allPermissions.Select(p =>
            {
                var fromRole = rolePermissionIds.Contains(p.Id);
                bool? overrideAllowed = overrides.TryGetValue(p.Id, out var allowed) ? allowed : null;
                var isEffective = overrideAllowed switch
                {
                    true => true,
                    false => false,
                    null => fromRole
                };
                return new UserPermissionViewDto
                {
                    PermissionId = p.Id,
                    PermissionName = p.Name,
                    FromRole = fromRole,
                    OverrideAllowed = overrideAllowed,
                    IsEffective = isEffective
                };
            }).ToList();
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
            return user;
        }

        public UserOut GetUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user is null or { IsRemoved: true })
                throw new KeyNotFoundException(nameof(id));
            return _mapper.Map<Users, UserOut>(user);
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
            return role;
        }

        public async Task<List<Permission>> GetUserPermissions(int userId, int roleId)
        {
            return await BuildEffectivePermissions(userId, new List<int> { roleId });
        }

        public async Task<List<Permission>> GetEffectivePermissions(int userId)
        {
            var roleIds = await _context.UserRoles.Where(x => x.UserId == userId).Select(x => x.RoleId).ToListAsync();
            if (roleIds.Count == 0)
                throw new UnauthorizedAccessException("المستخدم غير مرتبط بأي دور");
            return await BuildEffectivePermissions(userId, roleIds);
        }

        private async Task<List<Permission>> BuildEffectivePermissions(int userId, List<int> roleIds)
        {
            var rolePermissions = await _context.RolePermissions
                .Where(x => roleIds.Contains(x.RoleId))
                .Include(x => x.Permission)
                .Select(x => x.Permission)
                .ToListAsync();

            var overrides = await _context.UserPermissions
                .Where(x => x.UserId == userId)
                .Include(x => x.Permission)
                .ToListAsync();

            var denied = overrides.Where(x => !x.IsAllowed).Select(x => x.PermissionId).ToHashSet();
            var allowedExtra = overrides.Where(x => x.IsAllowed).Select(x => x.Permission).ToList();

            var result = rolePermissions
                .Where(p => !denied.Contains(p.Id))
                .Concat(allowedExtra)
                .GroupBy(p => p.Id)
                .Select(g => g.First())
                .ToList();

            return result;
        }

        public async Task<Role> GetRole(string RoleName)
        {
            var role = await _context.Roles.Where(x => x.Name == RoleName && x.IsRemoved == false).FirstOrDefaultAsync();
            if (role == null)
                throw new KeyNotFoundException("الدور غير موجود");
            return role;
        }

        public async Task<Role> GetRole(int RoleId)
        {
            var role = await _context.Roles.Where(x => x.Id == RoleId && x.IsRemoved == false).FirstOrDefaultAsync();
            if (role == null)
                throw new KeyNotFoundException("الدور غير موجود");
            return role;
        }

        public async Task<List<Role>> GetUserRoles(int userId)
        {
            var roles = await _context.UserRoles
                .Where(x => x.UserId == userId)
                .Include(x => x.Role)
                .Select(x => x.Role)
                .ToListAsync();
            return roles;
        }

        public async Task<List<Permission>> GetRolePermissions(int Id)
        {
            return await _context.RolePermissions
                .Where(x => x.RoleId == Id)
                .Include(x => x.Permission)
                .Select(x => x.Permission)
                .ToListAsync();
        }

        public async Task<Users?> GetFullUser(string Name)
        {
            Users? users = await _context.Users.Where(x => x.Username == Name && !x.IsRemoved)
                .Include(x => x.UserRoles)
                .FirstOrDefaultAsync();
            if (users is null)
                throw new KeyNotFoundException("المستخدم غير موجود");
            return users;
        }

        public async Task<Users?> GetUserByRefreshToken(string refreshToken)
        {
            var hashed = TokenHasher.Hash(refreshToken);
            Users? users = await _context.Users.Where(x => x.RefreshToken == hashed && !x.IsRemoved)
                .Include(x => x.UserRoles)
                .FirstOrDefaultAsync();
            if (users is null)
                throw new KeyNotFoundException("المستخدم غير موجود");
            return users;
        }

        public async Task UpdateUser(UpdateUserModel model, int updateUserId)
        {
            Users users = GetUserById(model.Id);
            if (!string.IsNullOrWhiteSpace(model.Username) && model.Username != users.Username)
            {
                var exists = await CheckUserExsist(model.Username);
                if (exists != null && exists.Id != users.Id)
                    throw new DuplicateException("اسم المستخدم مستخدم مسبقاً");
                users.Username = model.Username;
            }
            if (!string.IsNullOrWhiteSpace(model.Email) && model.Email != users.Email)
            {
                var emailExists = await _context.Users.AnyAsync(x => x.Email == model.Email && x.Id != users.Id);
                if (emailExists)
                    throw new DuplicateException("البريد الإلكتروني مستخدم مسبقاً");
                users.Email = model.Email;
            }
            if (model.IsActive.HasValue)
            {
                if (model.Id == updateUserId && !model.IsActive.Value)
                    throw new InvalidOperationException("لا يمكنك تعطيل حسابك الحالي");
                users.IsActive = model.IsActive.Value;
                if (!model.IsActive.Value)
                {
                    users.IsOnline = false;
                    users.RefreshToken = null;
                    users.RefreshTokenExpiryTime = null;
                    users.Token = null;
                }
            }

            users.UpdateDate = DateTime.UtcNow.AddHours(3);
            users.UpdateUserId = updateUserId;
            _context.Entry(users).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            if (model.RoleIds != null && model.RoleIds.Count > 0)
                await AssignUserRoles(model.Id, model.RoleIds, updateUserId);
        }

        public async Task DeleteUser(int userId, int removeUserId)
        {
            if (userId == removeUserId)
                throw new InvalidOperationException("لا يمكنك حذف حسابك الحالي");

            Users users = GetUserById(userId);
            users.IsRemoved = true;
            users.IsActive = false;
            users.IsOnline = false;
            users.RefreshToken = null;
            users.RefreshTokenExpiryTime = null;
            users.Token = null;
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
            return user;
        }

        public async Task UpdateUserpermission(int userId, List<UserPermissionDto> userPermission)
        {
            _ = GetUserById(userId);
            userPermission ??= new List<UserPermissionDto>();

            if (userPermission.Any(p => p.UserId != 0 && p.UserId != userId))
                throw new ArgumentException("جميع الصلاحيات يجب أن تخص نفس المستخدم");

            var existing = await _context.UserPermissions.Where(x => x.UserId == userId).ToListAsync();
            _context.UserPermissions.RemoveRange(existing);

            var toAdd = userPermission
                .GroupBy(p => p.PermissionId)
                .Select(g => g.First())
                .Select(dto => new UserPermissions
                {
                    UserId = userId,
                    PermissionId = dto.PermissionId,
                    IsAllowed = dto.IsAllowed
                })
                .ToList();

            if (toAdd.Count > 0)
                await _context.UserPermissions.AddRangeAsync(toAdd);
            await _context.SaveChangesAsync();
        }
    }
}
