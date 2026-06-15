using System.ComponentModel.DataAnnotations;
using Validation.Attribute;

namespace SherdProject.DTO
{
    public class UserModel
    {
        public int Id { get; set; }
        [StringValidate(5, 100, false)]
        public string Username { get; set; }
        [StringValidate(5, 100, false)]
        public string Password { get; set; }
        [StringValidate(5, 100, false)]
        public string Email { get; set; }
    }
}
