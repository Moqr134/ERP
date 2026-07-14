using System.ComponentModel.DataAnnotations;

namespace ERPDto.WarehouseDto
{
    public class WarehouseModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المخزن مطلوب")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "اسم المخزن يجب أن يكون بين 2 و 100 حرف")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "رمز المخزن مطلوب")]
        [StringLength(30, MinimumLength = 1, ErrorMessage = "رمز المخزن يجب أن يكون بين 1 و 30 حرف")]
        public string Code { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Location { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(250)]
        public string? Notes { get; set; }
    }
}
