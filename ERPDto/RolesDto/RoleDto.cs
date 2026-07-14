using Validation.Attribute;

namespace ERPDto.RolesDto
{
    public class RoleDto
    {
        public int Id { get; set; }
        [StringValidate(2, 100, false)]
        public string Name { get; set; } = string.Empty;
        [StringValidate(-1, 500, true)]
        public string Description { get; set; } = string.Empty;
    }
}
