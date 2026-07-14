using System.ComponentModel.DataAnnotations;
using Validation.Attribute;

namespace ERPDto.UserDto
{
    public class UpdateUserModel
    {
        [Range(1, int.MaxValue)]
        public int Id { get; set; }

        [StringValidate(3, 16, true)]
        public string? Username { get; set; }

        [EmailAddress]
        [StringValidate(5, 100, true)]
        public string? Email { get; set; }

        public bool? IsActive { get; set; }

        /// <summary>Optional: replace user roles when provided.</summary>
        public List<int>? RoleIds { get; set; }
    }
}
