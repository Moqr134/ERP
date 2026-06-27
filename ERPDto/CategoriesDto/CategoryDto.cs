using Validation.Attribute;

namespace ERPDto.CategoriesDto
{
    public class CategoryDto
    {
        public int Id { get; set; }
        [StringValidate(3,20,false)]
        public string Name { get; set; }
        [StringValidate(4,50,true)]
        public string Description { get; set; }
    }
}
