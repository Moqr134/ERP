using AutoMapper;
using ERP_API.App.IService;
using ERP_API.Domin.WarehouseEntity;
using ERP_API.Infrastructure.Services;
using ERPDto.WarehouseDto;
using Infrastructure.AppException;
using Infrastructure.ORM;
using Infrastructure.Service;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.App.Service
{
    public class WarehouseService : MasterService, IScopped, IWarehouseService
    {
        public WarehouseService(DBContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<List<WarehouseDto>> GetAllWarehousesAsync()
        {
            return await _context.Warehouses
                .AsNoTracking()
                .OrderBy(w => w.Name)
                .Select(w => new WarehouseDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    Code = w.Code,
                    Location = w.Location,
                    PhoneNumber = w.PhoneNumber,
                    IsActive = w.IsActive,
                    Notes = w.Notes,
                    ProductCount = _context.Products.Count(p => p.WarehouseId == w.Id && !p.IsRemoved)
                })
                .ToListAsync();
        }

        public async Task<WarehouseDto> GetWarehouseByIdAsync(int id)
        {
            var warehouse = await GetWarehouseAsync(id);
            if (warehouse is null)
                throw new KeyNotFoundException("المخزن غير موجود");

            return MapDto(warehouse,
                await _context.Products.CountAsync(p => p.WarehouseId == id && !p.IsRemoved));
        }

        public async Task AddWarehouseAsync(WarehouseModel model, int createUserId)
        {
            var code = (model.Code ?? string.Empty).Trim();
            var name = (model.Name ?? string.Empty).Trim();

            if (await _context.Warehouses.AnyAsync(w => w.Code == code && !w.IsRemoved))
                throw new DuplicateException("رمز المخزن مستخدم بالفعل");
            if (await _context.Warehouses.AnyAsync(w => w.Name == name && !w.IsRemoved))
                throw new DuplicateException("اسم المخزن مستخدم بالفعل");

            var warehouse = new Warehouse
            {
                Name = name,
                Code = code,
                Location = string.IsNullOrWhiteSpace(model.Location) ? null : model.Location.Trim(),
                PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim(),
                IsActive = model.IsActive,
                Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim(),
                CreateDate = DateTime.UtcNow.AddHours(3),
                CreateUserId = createUserId
            };

            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();
        }

        public async Task EditWarehouseAsync(WarehouseModel model, int updateUserId)
        {
            var warehouse = await GetWarehouseAsync(model.Id);
            if (warehouse is null)
                throw new KeyNotFoundException("المخزن غير موجود");

            var code = (model.Code ?? string.Empty).Trim();
            var name = (model.Name ?? string.Empty).Trim();

            if (await _context.Warehouses.AnyAsync(w => w.Code == code && w.Id != model.Id && !w.IsRemoved))
                throw new DuplicateException("رمز المخزن مستخدم في مخزن آخر");
            if (await _context.Warehouses.AnyAsync(w => w.Name == name && w.Id != model.Id && !w.IsRemoved))
                throw new DuplicateException("اسم المخزن مستخدم في مخزن آخر");

            warehouse.Name = name;
            warehouse.Code = code;
            warehouse.Location = string.IsNullOrWhiteSpace(model.Location) ? null : model.Location.Trim();
            warehouse.PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim();
            warehouse.IsActive = model.IsActive;
            warehouse.Notes = string.IsNullOrWhiteSpace(model.Notes) ? null : model.Notes.Trim();
            warehouse.UpdateDate = DateTime.UtcNow.AddHours(3);
            warehouse.UpdateUserId = updateUserId;

            _context.Warehouses.Entry(warehouse).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteWarehouseAsync(int id, int deleteUserId)
        {
            var warehouse = await GetWarehouseAsync(id);
            if (warehouse is null)
                throw new KeyNotFoundException("المخزن غير موجود");

            var linkedProducts = await _context.Products.CountAsync(p => p.WarehouseId == id && !p.IsRemoved);
            if (linkedProducts > 0)
                throw new LogicException("لا يمكن حذف المخزن لوجود منتجات مرتبطة به");

            warehouse.IsRemoved = true;
            warehouse.RemoveDate = DateTime.UtcNow.AddHours(3);
            warehouse.RemoveUserId = deleteUserId;

            _context.Warehouses.Entry(warehouse).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        private async Task<Warehouse?> GetWarehouseAsync(int id)
            => await _context.Warehouses.FirstOrDefaultAsync(w => w.Id == id && !w.IsRemoved);

        private static WarehouseDto MapDto(Warehouse w, int productCount) => new()
        {
            Id = w.Id,
            Name = w.Name,
            Code = w.Code,
            Location = w.Location,
            PhoneNumber = w.PhoneNumber,
            IsActive = w.IsActive,
            Notes = w.Notes,
            ProductCount = productCount
        };
    }
}
