using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validation.Attribute;

namespace ERPDto.UserDto
{
    public class UpdateUserModel
    {

        public int Id { get; set; }
        [StringValidate(-1,-1,true)]
        public string Username { get; set; }
        [StringValidate(-1,-1,true)]
        public required string Email { get; set; }
    }
}
