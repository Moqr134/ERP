using System.ComponentModel.DataAnnotations;
using Validation.Attribute;

namespace ERPDto.UserDto
{
    public class RegisterModel
    {
        [StringValidate(5, 100, false)]
        public string Username { get; set; } = string.Empty;
        [StringValidate(5, 100, false)]
        public string Password { get; set; } = string.Empty;
        [EmailAddress]
        [StringValidate(5, 100, false)]
        public string Email { get; set; } = string.Empty;
        [Range(1, int.MaxValue)]
        public int Role { get; set; }
    }
}
