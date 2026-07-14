using ERPDto.UserDto;

namespace ERPDto.RolesDto
{
    public class RolePermissionViewDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public List<PermissionDto> Permissions { get; set; } = new();
    }
}
