using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validation.Attribute;

namespace ERPDto.UserDto
{
    public class RegisterModel
    {
        [StringValidate(5, 100, false)]
        public string Username { get; set; }
        [StringValidate(5, 100, false)]
        public string Password { get; set; }
        [StringValidate(5, 100, false)]
        public string Email { get; set; }
        public int Role { get; set; }
    }
}
