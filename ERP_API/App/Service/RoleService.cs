using AutoMapper;
using ERP_API.App.IService;
using ERP_API.Domin.RoleEntity;
using ERP_API.Infrastructure.Services;
using ERPDto.RolesDto;
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
            if(role != null)
            {
                throw new DuplicateException("الدور موجود بالفعل.");
            }
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
            {
                throw new KeyNotFoundException("لم يتم العثور على الدور بالمعرف المحدد.");
            }
            if(role.Name!=null)
                role.Name = roleDto.Name;
            if(role.Description!=null)
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
            {
                throw new KeyNotFoundException("لم يتم العثور على الدور بالمعرف المحدد.");
            }
            role.IsRemoved = true;
            role.UpdateDate = DateTime.UtcNow.AddHours(3);
            role.UpdateUserId = deleteUserId;
            _context.Entry(role).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        public async Task<List<RoleDto>> GetAllRoles()
        {
            List<RoleDto> roles = _context.Roles.Where(r => !r.IsRemoved)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description
            }).ToList();
            return roles;
        }
        public async Task<Role?> GetRoleById(int id)
        {
            Role? role = _context.Roles.FirstOrDefault(r => r.Id == id && !r.IsRemoved);
            return role;
        }
        private async Task<Role?> GetRoleByName(string roleName)
        {
            Role? role = _context.Roles.FirstOrDefault(r => r.Name == roleName && !r.IsRemoved);
            return role;
        }
    }
}
