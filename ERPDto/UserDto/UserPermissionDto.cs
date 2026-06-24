using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPDto.UserDto
{
    public class UserPermissionDto
    {
        public int UserId { get; set; }
        public int PermissionId { get; set; }
        public bool IsAllowed { get; set; }
    }
}
