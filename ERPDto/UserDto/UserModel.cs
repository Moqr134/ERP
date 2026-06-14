using Validation.Attribute;

namespace SherdProject.DTO
{
    public class UserModel
    {
        public int Id { get; set; }
        [StringValidate]
        public string Username { get; set; }
        [StringValidate]
        public string Password { get; set; }
    }
}
