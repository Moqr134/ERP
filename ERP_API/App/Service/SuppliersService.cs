using AutoMapper;
using ERP_API.App.IService;
using ERP_API.Domin.SuppliersEntity;
using ERP_API.Infrastructure.Services;
using ERPDto.Suppliers;
using Infrastructure.AppException;
using Infrastructure.ORM;
using Infrastructure.Service;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.App.Service
{
    public class SuppliersService : MasterService, IScopped, ISuppliersService
    {
        public SuppliersService(DBContext context, IMapper mapper) : base(context, mapper)
        {
        }
        private async Task<Suppliers?> GetSuppliersAsync(string Name)
        {
            Suppliers? suppliers = await _context.Suppliers.FirstOrDefaultAsync(s => s.CompanyName == Name && !s.IsRemoved);
            return suppliers;
        }
        private async Task<Suppliers?> GetSuppliersAsync(int supplierId)
        {
            Suppliers? suppliers = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == supplierId && !s.IsRemoved);
            return suppliers;
        }
        public async Task AddSupplires(SuppliersModel supplier, int createId)
        {
            Suppliers? suppliers = await GetSuppliersAsync(supplier.CompanyName);
            if(suppliers != null)
            {
                throw new DuplicateException("المورد موجود بالفعل");
            }
            suppliers = _mapper.Map<Suppliers>(supplier);
            suppliers.CreateUserId = createId;
            suppliers.CreateDate = DateTime.UtcNow.AddHours(3);
            _context.Suppliers.Add(suppliers);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteSupplires(int supplierId, int deleteId)
        {
            Suppliers? suppliers= await GetSuppliersAsync(supplierId);
            if(suppliers == null) 
                throw new KeyNotFoundException("المورد غير موجود");
            suppliers.RemoveDate = DateTime.UtcNow.AddHours(3);
            suppliers.RemoveUserId = deleteId;
            suppliers.IsRemoved = true;
            _context.Suppliers.Entry(suppliers).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        public async Task EditSupplires(SuppliersModel supplier, int updateId)
        {
            Suppliers? existingSupplier = await GetSuppliersAsync(supplier.Id);
            if (existingSupplier == null)
            {
                throw new KeyNotFoundException("المورد غير موجود");
            }

            _mapper.Map(supplier, existingSupplier);
            existingSupplier.UpdateUserId = updateId;
            existingSupplier.UpdateDate = DateTime.UtcNow.AddHours(3);
            _context.Suppliers.Entry(existingSupplier).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        public async Task<List<SuppliersDto>> GetAllSupplires()
        {
            List<Suppliers> suppliersList = await _context.Suppliers.Where(s => !s.IsRemoved).ToListAsync();
            return suppliersList.Select(s => _mapper.Map<SuppliersDto>(s)).ToList();
        }
        public async Task<SuppliersDto> GetSuppliresById(int supplierId)
        {
            Suppliers? suppliers = await GetSuppliersAsync(supplierId);
            if (suppliers == null)
            {
                throw new KeyNotFoundException("المورد غير موجود");
            }
            return _mapper.Map<SuppliersDto>(suppliers);
        }
    }
}
