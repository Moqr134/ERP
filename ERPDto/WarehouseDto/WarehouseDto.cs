namespace ERPDto.WarehouseDto
{
    public class WarehouseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
        public int ProductCount { get; set; }
    }
}
