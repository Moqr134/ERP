using AutoMapper;
using ERP_API.App.IService;
using ERP_API.Domin.PermartionEntity;
using ERP_API.Domin.PermissionsEntity;
using ERP_API.Domin.RoleEntity;
using ERP_API.Infrastructure.Services;
using ERPDto.RolesDto;
using ERPDto.UserDto;
using Infrastructure.AppException;
using Infrastructure.ORM;
using Infrastructure.Service;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.App.Service
{
    public class RoleService : MasterService, IScopped, IRoleService
    {
        public RoleService(DBContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task CreateRole(RoleDto roleDto, int createUserId)
        {
            Role? role = await GetRoleByName(roleDto.Name);
            if (role != null)
                throw new DuplicateException("الدور موجود بالفعل.");

            role = _mapper.Map<Role>(roleDto);
            role.CreateDate = DateTime.UtcNow.AddHours(3);
            role.CreateUserId = createUserId;
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRole(int UpdateUserId, RoleDto roleDto)
        {
            Role? role = await GetRoleById(roleDto.Id);
            if (role == null)
                throw new KeyNotFoundException("لم يتم العثور على الدور بالمعرف المحدد.");

            if (roleDto.Name != null)
                role.Name = roleDto.Name;
            if (roleDto.Description != null)
                role.Description = roleDto.Description;
            role.UpdateDate = DateTime.UtcNow.AddHours(3);
            role.UpdateUserId = UpdateUserId;
            _context.Entry(role).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteRole(int id, int deleteUserId)
        {
            Role? role = await GetRoleById(id);
            if (role == null)
                throw new KeyNotFoundException("لم يتم العثور على الدور بالمعرف المحدد.");

            var assigned = await _context.UserRoles.AnyAsync(x => x.RoleId == id);
            if (assigned)
                throw new InvalidOperationException("لا يمكن حذف الدور لأنه مرتبط بمستخدمين");

            role.IsRemoved = true;
            role.UpdateDate = DateTime.UtcNow.AddHours(3);
            role.UpdateUserId = deleteUserId;
            _context.Entry(role).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task<List<RoleDto>> GetAllRoles()
        {
            return await _context.Roles.Where(r => !r.IsRemoved)
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description
                }).ToListAsync();
        }

        public async Task<List<PermissionDto>> GetAllPermissions()
        {
            return await _context.Permissions
                .OrderBy(p => p.Name)
                .Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name
                })
                .ToListAsync();
        }

        public async Task<RolePermissionViewDto> GetRolePermissions(int roleId)
        {
            Role? role = await GetRoleById(roleId);
            if (role == null)
                throw new KeyNotFoundException("لم يتم العثور على الدور بالمعرف المحدد.");

            var permissions = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Include(rp => rp.Permission)
                .Select(rp => new PermissionDto
                {
                    Id = rp.Permission.Id,
                    Name = rp.Permission.Name
                })
                .ToListAsync();

            return new RolePermissionViewDto
            {
                RoleId = role.Id,
                RoleName = role.Name,
                Permissions = permissions
            };
        }

        private async Task<Permission?> GetPermission(int id)
        {
            return await _context.Permissions.FindAsync(id);
        }

        public async Task<Role?> GetRoleById(int id)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.Id == id && !r.IsRemoved);
        }

        private async Task<Role?> GetRoleByName(string roleName)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName && !r.IsRemoved);
        }

        public async Task CreateRolePermission(int roleId, List<int> permissionIds)
        {
            Role? role = await GetRoleById(roleId);
            if (role == null)
                throw new KeyNotFoundException("لم يتم العثور على الدور بالمعرف المحدد.");

            permissionIds ??= new List<int>();
            foreach (var id in permissionIds.Distinct())
            {
                var permission = await GetPermission(id);
                if (permission == null)
                    throw new KeyNotFoundException($"لم يتم العثور على الصلاحية بالمعرف {id}.");
            }

            var existing = await _context.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync();
            _context.RolePermissions.RemoveRange(existing);

            var newItems = permissionIds.Distinct().Select(permissionId => new RolePermissions
            {
                RoleId = roleId,
                PermissionId = permissionId
            }).ToList();

            await _context.RolePermissions.AddRangeAsync(newItems);
            await _context.SaveChangesAsync();
        }
    }
}
