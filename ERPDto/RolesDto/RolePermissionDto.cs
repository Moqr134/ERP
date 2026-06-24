using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ERPDto.RolesDto
{
    public class RolePermissionDto
    {
        public int RoleId { get; set; }
        public List<int> PermissionIds { get; set; }
    }
}
