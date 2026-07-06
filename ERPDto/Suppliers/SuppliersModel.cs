using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Validation.Attribute;

namespace ERPDto.Suppliers
{
    public class SuppliersModel
    {
        public int Id { get; set; }
        [StringValidate(3,20,false)]
        public string CompanyName { get; set; }
        [StringValidate(3, 20, false)]
        public string ContactName { get; set; }
        [StringValidate(3, 20, false)]
        public string PhoneNumper { get; set; }
    }
}
